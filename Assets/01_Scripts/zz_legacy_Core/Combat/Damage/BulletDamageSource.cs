using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat.Projectiles;

/// <summary>
/// 범용 탄환. BulletPhysicsConfig로 이동/수명/충돌 조합.
/// </summary>
public class BulletDamageSource : DamageSourceEntity
{
    [Header("Bullet Configuration")]
    [SerializeField] private BulletPhysicsConfig bulletConfig;

    #region Private Fields
    private Collider2D mainCollider;

    private IMovementStrategy movementStrategy;
    private ILifetimeStrategy lifetimeStrategy;

    private Vector2 moveDirection = Vector2.right;
    private float currentSpeed;
    private float traveledDistance;
    private int hitCount;
    private bool isInitialized;
    private float moveTimer;
    private Vector2 lastPosition;

    private readonly Dictionary<CollisionActionType, int> actionCounts = new();

    private Coroutine lifetimeCoroutine;
    private Coroutine movementCoroutine;
    #endregion

    #region Initialization
    public override void Initialize()
    {
        if (bulletConfig != null) InitializeFromConfig();
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
            Debug.LogError($"{name}: BulletPhysicsConfig is null!");
            return;
        }

        damage = Mathf.RoundToInt(bulletConfig.Damage);
        currentSpeed = bulletConfig.InitialSpeed;
        SetTargetLayers(bulletConfig.TargetLayers);

        CopyWhitelistBlacklist();

        CreateStrategies();
        SetupPhysics();
        SetupCollider();
        ResetState();

        isInitialized = true;
    }

    private void CopyWhitelistBlacklist()
    {
        foreach (var e in bulletConfig.Whitelist) AddToWhitelist(e);
        foreach (var e in bulletConfig.Blacklist) AddToBlacklist(e);
    }

    private void CreateStrategies()
    {
        movementStrategy = BulletStrategyFactory.CreateMovementStrategy(bulletConfig.MovementType);
        lifetimeStrategy = BulletStrategyFactory.CreateLifetimeStrategy(bulletConfig.LifetimeType);
    }

    private void SetupPhysics()
    {
        if (entityRigidbody == null) return;
        entityRigidbody.mass = bulletConfig.Mass;
        entityRigidbody.linearDamping = bulletConfig.Drag;
        entityRigidbody.gravityScale = bulletConfig.UseGravity ? bulletConfig.GravityScale : 0f;
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

            mainCollider = capsule;
            mainCollider.enabled = false;
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

        InitializeStrategies();
        StartLifecycleSystem();
        StartMovementSystem();
        UpdatePhysicsVelocity();

        if (mainCollider != null)
            mainCollider.enabled = true; // ✅ 초기화 완료 후 충돌 활성
    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void InitializeStrategies()
    {
        movementStrategy?.Initialize(entityRigidbody, bulletConfig, moveDirection);
        lifetimeStrategy?.Initialize(bulletConfig, DestroySelf);
    }

    private void StartLifecycleSystem()
    {
        if (bulletConfig.LifetimeType != LifetimeType.Infinite)
            lifetimeCoroutine = StartCoroutine(LifetimeUpdateCoroutine());
    }

    private void StartMovementSystem()
    {
        if (ShouldRunMovementUpdate())
            movementCoroutine = StartCoroutine(MovementUpdateCoroutine());
    }

    private bool ShouldRunMovementUpdate()
    {
        return bulletConfig.EnableUpdate && movementStrategy?.RequiresUpdate == true;
    }

    private IEnumerator LifetimeUpdateCoroutine()
    {
        while (gameObject.activeInHierarchy && isInitialized)
        {
            float dt = Time.deltaTime;

            Vector2 currentPos = transform.position;
            traveledDistance += Vector2.Distance(lastPosition, currentPos);
            lastPosition = currentPos;

            lifetimeStrategy?.UpdateLifetime(dt, traveledDistance, hitCount);
            yield return null;
        }
    }

    private IEnumerator MovementUpdateCoroutine()
    {
        while (gameObject.activeInHierarchy && isInitialized)
        {
            float dt = Time.deltaTime;
            moveTimer += dt;

            movementStrategy?.UpdateMovement(dt, moveTimer);
            yield return null;
        }
    }
    #endregion

    #region Collision Handling
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized || bulletConfig == null) return; // ✅ 초기화 전/Config 없음 → 무시

        bool isObstacle = (bulletConfig.ObstacleLayers.value & (1 << other.gameObject.layer)) != 0;

        if (isObstacle)
        {
            HandleObstacleCollision(other);
            return;
        }

        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target != null && CanDamage(target))
        {
            HandleEntityCollision(target);
        }
    }

    private void HandleEntityCollision(BaseEntity target)
    {
        var damageData = GenerateDamageData(target);
        target.TakeDamage(damageData);
        hitCount++;

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
        var actions = new List<ICollisionAction>();

        foreach (var cfg in configs)
        {
            if (ShouldSkipAction(cfg)) continue;

            var action = CreateSingleAction(cfg);
            if (action != null)
            {
                actions.Add(action);
                IncrementActionCount(cfg.actionType);
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

    private ICollisionAction CreateSingleAction(BulletPhysicsConfig.CollisionActionConfig cfg)
    {
        return cfg.actionType switch
        {
            CollisionActionType.Penetrate   => new PenetrateAction(),
            CollisionActionType.Bounce      => new BounceAction(cfg.dampingFactor),
            CollisionActionType.Stop        => new StopAction(),
            CollisionActionType.Destroy     => new DestroyAction(),
            CollisionActionType.SpawnEntity => new SpawnEntityAction(cfg.entityPrefab, cfg.spawnCount, cfg.spawnOffset),
            _ => null
        };
    }

    private void IncrementActionCount(CollisionActionType type) => actionCounts[type]++;

    private List<CollisionActionResult> ExecuteActionsInOrder(List<ICollisionAction> actions, CollisionContext context)
    {
        var results = new List<CollisionActionResult>();
        foreach (var action in actions)
            results.Add(action.Execute(context));
        return results;
    }

    private void ApplyFinalResult(List<CollisionActionResult> results)
    {
        bool shouldContinue = true;
        bool shouldDestroy = false;
        Vector2 finalDirection = moveDirection;
        float finalSpeedMultiplier = 1f;

        foreach (var r in results)
        {
            shouldContinue &= r.continueBullet;
            shouldDestroy  |= r.destroyBullet;
            if (r.newDirection != Vector2.zero) finalDirection = r.newDirection;
            finalSpeedMultiplier *= r.speedMultiplier;
        }

        ApplyCollisionResult(shouldContinue, shouldDestroy, finalDirection, finalSpeedMultiplier);
    }

    private void ApplyCollisionResult(bool shouldContinue, bool shouldDestroy, Vector2 finalDirection, float speedMultiplier)
    {
        if (shouldDestroy)
        {
            DestroySelf();
            return;
        }

        if (!shouldContinue)
        {
            StopBullet();
            return;
        }

        UpdateMovementFromCollision(finalDirection, speedMultiplier);
    }

    private void StopBullet()
    {
        if (entityRigidbody != null) entityRigidbody.linearVelocity = Vector2.zero;
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
            entityRigidbody.linearVelocity = moveDirection * currentSpeed;
    }
    #endregion

    #region Public Interface
    public Vector2 GetDirection() => moveDirection;
    public float GetCurrentSpeed() => currentSpeed;
    public void SetSpeed(float newSpeed) => currentSpeed = newSpeed;
    #endregion

    #region Pool Hooks / Cleanup
    // 공통 파괴 경로 사용
    protected override void DestroySelf()
    {
        StopAllCoroutines();
        base.DestroySelf();
    }
    public override void OnSpawned()
    {
        if (mainCollider == null && physicsContainer?.MainCollider != null)
            mainCollider = physicsContainer.MainCollider as Collider2D;

        if (mainCollider != null)
            mainCollider.enabled = false; // ✅ 스폰 직후 비활성
    }

    public override void OnDespawned()
    {
        // 종료 전 경량 정리(Heavy 정리는 ResetForPool에서)
        StopAllCoroutines();
    }

    public override void ResetForPool()
    {
        base.ResetForPool();
        isInitialized = false;
        bulletConfig = null;
        movementStrategy = null;
        lifetimeStrategy = null;

        if (entityRigidbody != null)
        {
            entityRigidbody.linearVelocity = Vector2.zero;
            entityRigidbody.angularVelocity = 0f;
        }
        if (mainCollider != null)
            mainCollider.enabled = false; // ✅ 풀 복귀 시 비활성

        ResetState();
    }
    #endregion
}
