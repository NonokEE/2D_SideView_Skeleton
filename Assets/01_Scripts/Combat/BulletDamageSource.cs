using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat.Projectiles;

public class BulletDamageSource : DamageSourceEntity
{
    [Header("BulletDamageSource Configuration")]
    [SerializeField] private BulletPhysicsConfig bulletConfig;
    
    // Movement 전략만 유지 (충돌은 CollisionAction으로 처리)
    private IMovementStrategy movementStrategy;
    private ILifetimeStrategy lifetimeStrategy;
    
    // 런타임 상태 변수들
    private Vector2 moveDirection = Vector2.right;
    private float currentSpeed;
    private float traveledDistance;
    private int hitCount;
    private bool isInitialized = false;
    private float moveTimer;
    private Vector2 lastPosition;
    
    // CollisionAction 실행 카운터
    private Dictionary<CollisionActionType, int> actionCounts = new();
    
    // 생명주기 관리
    private Coroutine updateCoroutine;
    
    public override void Initialize()
    {
        if (bulletConfig != null)
        {
            InitializeFromConfig();
        }
    }
    
    public virtual void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon, BulletPhysicsConfig config)
    {
        base.Initialize(sourceOwner, sourceWeapon);
        bulletConfig = config;
        InitializeFromConfig();
    }
    
    private void InitializeFromConfig()
    {
        if (bulletConfig == null) return;
        
        // 기본 설정
        damage = Mathf.RoundToInt(bulletConfig.Damage);
        currentSpeed = bulletConfig.InitialSpeed;
        SetTargetLayers(bulletConfig.TargetLayers);
        
        // Whitelist/Blacklist 복사
        foreach (var entity in bulletConfig.Whitelist)
            AddToWhitelist(entity);
        foreach (var entity in bulletConfig.Blacklist)
            AddToBlacklist(entity);
        
        // 전략 객체들 생성 (Movement와 Lifetime만)
        CreateStrategies();
        
        // 물리 설정
        SetupPhysics();
        SetupCollider();
        
        // 상태 초기화
        ResetState();
        
        isInitialized = true;
    }
    
    private void CreateStrategies()
    {
        movementStrategy = BulletStrategyFactory.CreateMovementStrategy(bulletConfig.MovementType);
        lifetimeStrategy = BulletStrategyFactory.CreateLifetimeStrategy(bulletConfig.LifetimeType);
    }
    
    private void SetupPhysics()
    {
        if (entityRigidbody != null)
        {
            entityRigidbody.mass = bulletConfig.Mass;
            entityRigidbody.linearDamping = bulletConfig.Drag;
            entityRigidbody.gravityScale = bulletConfig.UseGravity ? bulletConfig.GravityScale : 0f;
        }
    }
    
    private void SetupCollider()
    {
        if (physicsContainer?.MainCollider is CapsuleCollider2D capsule)
        {
            capsule.isTrigger = true;
            capsule.size = bulletConfig.ColliderSize;
            capsule.offset = bulletConfig.ColliderOffset;
            capsule.direction = bulletConfig.ColliderDirection == BulletCapsuleDirection.Vertical 
                ? CapsuleDirection2D.Vertical : CapsuleDirection2D.Horizontal;
        }
    }
    
    private void ResetState()
    {
        traveledDistance = 0f;
        hitCount = 0;
        moveTimer = 0f;
        lastPosition = transform.position;
        actionCounts.Clear();
    }
    
    public virtual void SetDirection(Vector2 direction)
    {
        if (!isInitialized) return;
        
        moveDirection = direction.normalized;
        
        // 탄환 회전 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        // 전략 초기화
        movementStrategy?.Initialize(entityRigidbody, bulletConfig, moveDirection);
        
        // ✅ 생명주기는 항상 독립 실행
        StartLifetimeSystem();
        
        // Movement Update는 필요한 경우에만
        if (ShouldRunMovementUpdate())
        {
            updateCoroutine = StartCoroutine(MovementUpdateCoroutine());
        }
    }

    private void StartLifetimeSystem()
    {
        if (lifetimeStrategy != null && bulletConfig.LifetimeType != LifetimeType.Infinite)
        {
            lifetimeStrategy.Initialize(bulletConfig, DestroyBullet);
            StartCoroutine(LifetimeUpdateCoroutine());
        }
    }

    private bool ShouldRunMovementUpdate()
    {
        return bulletConfig.EnableUpdate && movementStrategy?.RequiresUpdate == true;
    }

    // 생명주기 전용 코루틴
    private IEnumerator LifetimeUpdateCoroutine()
    {
        while (gameObject.activeInHierarchy && isInitialized)
        {
            float deltaTime = Time.deltaTime;
            
            // 거리 계산
            Vector2 currentPosition = transform.position;
            traveledDistance += Vector2.Distance(lastPosition, currentPosition);
            lastPosition = currentPosition;
            
            // 생명주기 업데이트
            lifetimeStrategy?.UpdateLifetime(deltaTime, traveledDistance, hitCount);
            
            yield return null;
        }
    }

    // Movement 전용 코루틴 (기존 UpdateCoroutine에서 분리)
    private IEnumerator MovementUpdateCoroutine()
    {
        while (gameObject.activeInHierarchy && isInitialized)
        {
            float deltaTime = Time.deltaTime;
            moveTimer += deltaTime;
            
            // 이동 업데이트만
            movementStrategy?.UpdateMovement(deltaTime, moveTimer);
            
            yield return null;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 장애물 충돌 체크
        if ((bulletConfig.ObstacleLayers.value & (1 << other.gameObject.layer)) != 0)
        {
            HandleObstacleCollision(other);
            return;
        }
        
        // Entity 충돌 처리
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target != null && CanDamage(target))
        {
            HandleEntityCollision(target);
        }
    }
    
    private void HandleEntityCollision(BaseEntity target)
    {
        // 피해 적용
        DamageData damageData = GenerateDamageData(target);
        target.TakeDamage(damageData);
        
        hitCount++;
        
        // 충돌 컨텍스트 생성
        CollisionContext context = new CollisionContext
        {
            bullet = this,
            collider = target.GetComponent<Collider2D>(),
            targetEntity = target,
            layerMask = bulletConfig.TargetLayers,
            hitPoint = transform.position
        };
        
        // Enemy CollisionAction 실행
        ExecuteCollisionActions(bulletConfig.EnemyCollisionActions, context);
    }
    
    private void HandleObstacleCollision(Collider2D obstacle)
    {
        // 충돌 컨텍스트 생성
        CollisionContext context = new CollisionContext
        {
            bullet = this,
            collider = obstacle,
            targetEntity = null,
            layerMask = bulletConfig.ObstacleLayers,
            hitPoint = transform.position
        };
        
        // Wall CollisionAction 실행
        ExecuteCollisionActions(bulletConfig.WallCollisionActions, context);
    }
    
    private void ExecuteCollisionActions(BulletPhysicsConfig.CollisionActionConfig[] configs, CollisionContext context)
    {
        if (configs == null || configs.Length == 0) return;
        
        bool shouldContinue = true;
        bool shouldDestroy = false;
        Vector2 finalDirection = moveDirection;
        float finalSpeedMultiplier = 1f;
        
        var actions = CreateCollisionActions(configs);
        actions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        
        // ✅ 실행과 동시에 카운트 증가
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            var config = configs[i];
            
            // 액션 실행
            var result = action.Execute(context);
            
            // ✅ 실행 후 카운트 증가
            actionCounts[config.actionType]++;
            
            // 결과 조합
            shouldContinue &= result.continueBullet;
            shouldDestroy |= result.destroyBullet;
            
            if (result.newDirection != Vector2.zero)
                finalDirection = result.newDirection;
                
            finalSpeedMultiplier *= result.speedMultiplier;
        }
        
        ApplyCollisionResult(shouldContinue, shouldDestroy, finalDirection, finalSpeedMultiplier);
    }
    
    private void ApplyCollisionResult(bool shouldContinue, bool shouldDestroy, Vector2 finalDirection, float speedMultiplier)
    {
        if (shouldDestroy)
        {
            DestroyBullet();
            return;
        }
        
        if (!shouldContinue)
        {
            // 탄환 정지
            if (entityRigidbody != null)
                entityRigidbody.linearVelocity = Vector2.zero;
            return;
        }
        
        // 방향/속도 업데이트
        if (finalDirection != moveDirection)
        {
            moveDirection = finalDirection.normalized;
            UpdateRotation();
            movementStrategy?.SetDirection(moveDirection);
        }
        
        // 속도 적용
        currentSpeed *= speedMultiplier;
        if (entityRigidbody != null && movementStrategy?.RequiresUpdate != true)
        {
            // MovementStrategy가 Update를 사용하지 않는 경우 직접 속도 적용
            entityRigidbody.linearVelocity = moveDirection * currentSpeed;
        }
    }
    
    private List<ICollisionAction> CreateCollisionActions(BulletPhysicsConfig.CollisionActionConfig[] configs)
    {
        List<ICollisionAction> actions = new();
        
        foreach (var config in configs)
        {
            // ✅ 실행 전에 카운트 체크 (실행 후 증가로 변경)
            if (!actionCounts.ContainsKey(config.actionType))
                actionCounts[config.actionType] = 0;
                
            // 최대 실행 횟수에 도달했으면 스킵
            if (actionCounts[config.actionType] >= config.maxExecutions)
            {
                Debug.Log($"Action {config.actionType} reached max executions ({config.maxExecutions})");
                continue;
            }
            
            // Action 생성
            ICollisionAction action = config.actionType switch
            {
                CollisionActionType.Penetrate => new PenetrateAction(),
                CollisionActionType.Bounce => new BounceAction(config.dampingFactor),
                CollisionActionType.Stop => new StopAction(),
                CollisionActionType.Destroy => new DestroyAction(),
                CollisionActionType.SpawnEntity => new SpawnEntityAction(config.entityPrefab, config.spawnCount, config.spawnOffset),
                _ => new StopAction()
            };
            
            actions.Add(action);
        }
        
        return actions;
    }
    
    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    // CollisionAction에서 호출할 수 있는 메서드들
    public Vector2 GetDirection() => moveDirection;
    public float GetCurrentSpeed() => currentSpeed;
    public void SetSpeed(float newSpeed) => currentSpeed = newSpeed;
    
    protected virtual void DestroyBullet()
    {
        // 코루틴 정리
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        // Pool 반환 또는 파괴
        gameObject.SetActive(false);
    }
    
    public virtual void ResetForPool()
    {
        isInitialized = false;
        bulletConfig = null;
        
        // 전략 객체들 해제
        movementStrategy = null;
        lifetimeStrategy = null;
        
        // 코루틴 정리
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        // 물리 리셋
        if (entityRigidbody != null)
            entityRigidbody.linearVelocity = Vector2.zero;
            
        // 상태 리셋
        ResetState();
        
        // Whitelist/Blacklist 초기화
        whitelist.Clear();
        blacklist.Clear();
    }
}
