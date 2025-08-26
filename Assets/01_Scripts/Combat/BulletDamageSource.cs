using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat.Projectiles;

/// <summary>
/// 범용 탄환 시스템. BulletPhysicsConfig를 통해 다양한 탄환 효과 구현 가능.
/// Movement, Lifetime, CollisionAction을 조합하여 복잡한 탄환 동작 지원.
/// </summary>
public class BulletDamageSource : DamageSourceEntity
{
    [Header("Bullet Configuration")]
    [SerializeField] private BulletPhysicsConfig bulletConfig;
    
    #region Private Fields
    
    // Movement 전략 (충돌은 CollisionAction으로 처리)
    private IMovementStrategy movementStrategy;
    private ILifetimeStrategy lifetimeStrategy;
    
    // 런타임 상태
    private Vector2 moveDirection = Vector2.right;
    private float currentSpeed;
    private float traveledDistance;
    private int hitCount;
    private bool isInitialized = false;
    private float moveTimer;
    private Vector2 lastPosition;
    
    // CollisionAction 실행 관리
    private Dictionary<CollisionActionType, int> actionCounts = new();
    
    // 생명주기 관리
    private Coroutine lifetimeCoroutine;
    private Coroutine movementCoroutine;
    
    #endregion
    
    #region Initialization
    
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
        if (bulletConfig == null)
        {
            Debug.LogError($"{gameObject.name}: BulletPhysicsConfig is null!");
            return;
        }
        
        // 기본 설정
        damage = Mathf.RoundToInt(bulletConfig.Damage);
        currentSpeed = bulletConfig.InitialSpeed;
        SetTargetLayers(bulletConfig.TargetLayers);
        
        // 피아식별 설정
        CopyWhitelistBlacklist();
        
        // 시스템 초기화
        CreateStrategies();
        SetupPhysics();
        SetupCollider();
        ResetState();
        
        isInitialized = true;
    }
    
    private void CopyWhitelistBlacklist()
    {
        foreach (var entity in bulletConfig.Whitelist)
            AddToWhitelist(entity);
        foreach (var entity in bulletConfig.Blacklist)
            AddToBlacklist(entity);
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
    
    #endregion
    
    #region Movement & Lifecycle
    
    public virtual void SetDirection(Vector2 direction)
    {
        if (!isInitialized) return;
        
        moveDirection = direction.normalized;
        UpdateRotation();
        
        // 시스템 시작
        InitializeStrategies();
        StartLifecycleSystem();
        StartMovementSystem();
    }
    
    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    private void InitializeStrategies()
    {
        movementStrategy?.Initialize(entityRigidbody, bulletConfig, moveDirection);
        lifetimeStrategy?.Initialize(bulletConfig, DestroyBullet);
    }
    
    private void StartLifecycleSystem()
    {
        // 생명주기는 항상 독립 실행 (Enable Update와 무관)
        if (bulletConfig.LifetimeType != LifetimeType.Infinite)
        {
            lifetimeCoroutine = StartCoroutine(LifetimeUpdateCoroutine());
        }
    }
    
    private void StartMovementSystem()
    {
        // Movement Update는 필요한 경우에만
        if (ShouldRunMovementUpdate())
        {
            movementCoroutine = StartCoroutine(MovementUpdateCoroutine());
        }
    }
    
    private bool ShouldRunMovementUpdate()
    {
        return bulletConfig.EnableUpdate && movementStrategy?.RequiresUpdate == true;
    }
    
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
    
    private IEnumerator MovementUpdateCoroutine()
    {
        while (gameObject.activeInHierarchy && isInitialized)
        {
            float deltaTime = Time.deltaTime;
            moveTimer += deltaTime;
            
            // 이동 업데이트
            movementStrategy?.UpdateMovement(deltaTime, moveTimer);
            
            yield return null;
        }
    }
    
    #endregion
    
    #region Collision Handling
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 장애물 vs Entity 분류
        bool isObstacle = (bulletConfig.ObstacleLayers.value & (1 << other.gameObject.layer)) != 0;
        
        if (isObstacle)
        {
            HandleObstacleCollision(other);
        }
        else
        {
            BaseEntity target = other.GetComponent<BaseEntity>();
            if (target != null && CanDamage(target))
            {
                HandleEntityCollision(target);
            }
        }
    }
    
    private void HandleEntityCollision(BaseEntity target)
    {
        // 피해 적용
        DamageData damageData = GenerateDamageData(target);
        target.TakeDamage(damageData);
        hitCount++;
        
        // CollisionAction 실행
        ExecuteCollisionActions(bulletConfig.EnemyCollisionActions, CreateCollisionContext(target));
    }
    
    private void HandleObstacleCollision(Collider2D obstacle)
    {
        ExecuteCollisionActions(bulletConfig.WallCollisionActions, CreateCollisionContext(obstacle));
    }
    
    private CollisionContext CreateCollisionContext(BaseEntity target)
    {
        return new CollisionContext
        {
            bullet = this,
            collider = target.GetComponent<Collider2D>(),
            targetEntity = target,
            layerMask = bulletConfig.TargetLayers,
            hitPoint = transform.position
        };
    }
    
    private CollisionContext CreateCollisionContext(Collider2D obstacle)
    {
        return new CollisionContext
        {
            bullet = this,
            collider = obstacle,
            targetEntity = null,
            layerMask = bulletConfig.ObstacleLayers,
            hitPoint = transform.position
        };
    }
    
    #endregion
    
    #region CollisionAction System
    
    private void ExecuteCollisionActions(BulletPhysicsConfig.CollisionActionConfig[] configs, CollisionContext context)
    {
        if (configs == null || configs.Length == 0) return;
        
        var actions = CreateCollisionActions(configs);
        var results = ExecuteActionsInOrder(actions, context);
        ApplyFinalResult(results);
    }
    
    private List<ICollisionAction> CreateCollisionActions(BulletPhysicsConfig.CollisionActionConfig[] configs)
    {
        List<ICollisionAction> actions = new();
        
        foreach (var config in configs)
        {
            if (ShouldSkipAction(config)) continue;
            
            ICollisionAction action = CreateSingleAction(config);
            if (action != null)
            {
                actions.Add(action);
                IncrementActionCount(config.actionType);
            }
        }
        
        return actions.OrderBy(a => a.Priority).ToList();
    }
    
    private bool ShouldSkipAction(BulletPhysicsConfig.CollisionActionConfig config)
    {
        if (!actionCounts.ContainsKey(config.actionType))
            actionCounts[config.actionType] = 0;
            
        return actionCounts[config.actionType] >= config.maxExecutions;
    }
    
    private ICollisionAction CreateSingleAction(BulletPhysicsConfig.CollisionActionConfig config)
    {
        return config.actionType switch
        {
            CollisionActionType.Penetrate => new PenetrateAction(),
            CollisionActionType.Bounce => new BounceAction(config.dampingFactor),
            CollisionActionType.Stop => new StopAction(),
            CollisionActionType.Destroy => new DestroyAction(),
            CollisionActionType.SpawnEntity => new SpawnEntityAction(config.entityPrefab, config.spawnCount, config.spawnOffset),
            _ => null
        };
    }
    
    private void IncrementActionCount(CollisionActionType actionType)
    {
        actionCounts[actionType]++;
    }
    
    private List<CollisionActionResult> ExecuteActionsInOrder(List<ICollisionAction> actions, CollisionContext context)
    {
        List<CollisionActionResult> results = new();
        
        foreach (var action in actions)
        {
            // ✅ 전달받은 context 사용
            var result = action.Execute(context);
            results.Add(result);
        }
        
        return results;
    }
    
    private void ApplyFinalResult(List<CollisionActionResult> results)
    {
        bool shouldContinue = true;
        bool shouldDestroy = false;
        Vector2 finalDirection = moveDirection;
        float finalSpeedMultiplier = 1f;
        
        // 모든 결과 조합
        foreach (var result in results)
        {
            shouldContinue &= result.continueBullet;
            shouldDestroy |= result.destroyBullet;
            
            if (result.newDirection != Vector2.zero)
                finalDirection = result.newDirection;
                
            finalSpeedMultiplier *= result.speedMultiplier;
        }
        
        // 최종 적용
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
            StopBullet();
            return;
        }
        
        // 방향/속도 업데이트
        UpdateMovementFromCollision(finalDirection, speedMultiplier);
    }
    
    private void StopBullet()
    {
        if (entityRigidbody != null)
            entityRigidbody.linearVelocity = Vector2.zero;
    }
    
    private void UpdateMovementFromCollision(Vector2 newDirection, float speedMultiplier)
    {
        if (newDirection != moveDirection)
        {
            moveDirection = newDirection.normalized;
            UpdateRotation();
            movementStrategy?.SetDirection(moveDirection);
        }
        
        currentSpeed *= speedMultiplier;
        UpdatePhysicsVelocity();
    }
    
    private void UpdatePhysicsVelocity()
    {
        if (entityRigidbody != null && movementStrategy?.RequiresUpdate != true)
        {
            entityRigidbody.linearVelocity = moveDirection * currentSpeed;
        }
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// CollisionAction에서 사용할 수 있는 탄환 방향 정보
    /// </summary>
    public Vector2 GetDirection() => moveDirection;
    
    /// <summary>
    /// CollisionAction에서 사용할 수 있는 탄환 속도 정보
    /// </summary>
    public float GetCurrentSpeed() => currentSpeed;
    
    /// <summary>
    /// CollisionAction에서 탄환 속도 변경
    /// </summary>
    public void SetSpeed(float newSpeed) => currentSpeed = newSpeed;
    
    #endregion
    
    #region Cleanup
    
    protected virtual void DestroyBullet()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    
    public virtual void ResetForPool()
    {
        // Pool 사용을 위한 재설정
        isInitialized = false;
        bulletConfig = null;
        
        // 전략 해제
        movementStrategy = null;
        lifetimeStrategy = null;
        
        // 상태 리셋
        StopAllCoroutines();
        ResetPhysics();
        ResetState();
        ClearTargetLists();
    }
    
    private void ResetPhysics()
    {
        if (entityRigidbody != null)
            entityRigidbody.linearVelocity = Vector2.zero;
    }
    
    private void ClearTargetLists()
    {
        whitelist.Clear();
        blacklist.Clear();
    }
    
    #endregion
}
