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
        lifetimeStrategy?.Initialize(bulletConfig, DestroyBullet);
        
        // 업데이트 시작 (필요한 경우에만)
        if (ShouldRunUpdate())
        {
            updateCoroutine = StartCoroutine(UpdateCoroutine());
        }
    }
    
    private bool ShouldRunUpdate()
    {
        return bulletConfig.EnableUpdate && 
               (movementStrategy?.RequiresUpdate == true || 
                bulletConfig.LifetimeType != LifetimeType.Infinite);
    }
    
    private IEnumerator UpdateCoroutine()
    {
        while (gameObject.activeInHierarchy && isInitialized)
        {
            float deltaTime = Time.deltaTime;
            moveTimer += deltaTime;
            
            // 이동 업데이트
            movementStrategy?.UpdateMovement(deltaTime, moveTimer);
            
            // 거리 계산
            Vector2 currentPosition = transform.position;
            traveledDistance += Vector2.Distance(lastPosition, currentPosition);
            lastPosition = currentPosition;
            
            // 생명주기 업데이트
            lifetimeStrategy?.UpdateLifetime(deltaTime, traveledDistance, hitCount);
            
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
        
        // CollisionAction 생성 및 실행
        var actions = CreateCollisionActions(configs);
        
        // Priority 순으로 정렬하여 실행
        actions.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        
        foreach (var action in actions)
        {
            var result = action.Execute(context);
            
            // 결과 조합 (AND/OR 로직)
            shouldContinue &= result.continueBullet;
            shouldDestroy |= result.destroyBullet;
            
            // 방향 변경 (마지막 변경사항 적용)
            if (result.newDirection != Vector2.zero)
                finalDirection = result.newDirection;
                
            // 속도 배율 누적
            finalSpeedMultiplier *= result.speedMultiplier;
        }
        
        // 최종 결과 적용
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
            // 최대 실행 횟수 체크
            if (!actionCounts.ContainsKey(config.actionType))
                actionCounts[config.actionType] = 0;
                
            if (actionCounts[config.actionType] >= config.maxExecutions)
                continue;
                
            actionCounts[config.actionType]++;
            
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
