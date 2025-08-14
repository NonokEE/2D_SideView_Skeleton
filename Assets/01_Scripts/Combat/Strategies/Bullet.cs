using UnityEngine;
using System.Collections;
using Combat.Projectiles;

public class Bullet : DamageSourceEntity
{
    [Header("Bullet Configuration")]
    [SerializeField] private BulletPhysicsConfig bulletConfig;
    
    // 전략 객체들
    private IMovementStrategy movementStrategy;
    private ILifetimeStrategy lifetimeStrategy;
    private ICollisionStrategy enemyCollisionStrategy;
    private ICollisionStrategy wallCollisionStrategy;
    private IEffectStrategy deathEffectStrategy;
    
    // 런타임 상태 변수들
    private Vector2 moveDirection = Vector2.right;
    private float currentSpeed;
    private float traveledDistance;
    private int hitCount;
    private int penetrationCount;
    private int bounceCount;
    private bool isInitialized = false;
    private float moveTimer;
    private Vector2 lastPosition;
    
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
        
        // 전략 객체들 생성
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
        enemyCollisionStrategy = BulletStrategyFactory.CreateCollisionStrategy(bulletConfig.EnemyHitBehavior);
        wallCollisionStrategy = BulletStrategyFactory.CreateCollisionStrategy(bulletConfig.WallHitBehavior);
        deathEffectStrategy = BulletStrategyFactory.CreateEffectStrategy(bulletConfig.DeathEffect);
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
        penetrationCount = 0;
        bounceCount = 0;
        moveTimer = 0f;
        lastPosition = transform.position;
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
        
        // 충돌 전략에 따른 처리
        CollisionResult result = enemyCollisionStrategy.HandleEntityHit(target, ref hitCount, ref penetrationCount);
        HandleCollisionResult(result);
    }
    
    private void HandleObstacleCollision(Collider2D obstacle)
    {
        CollisionResult result = wallCollisionStrategy.HandleObstacleHit(obstacle, ref bounceCount, ref moveDirection, ref currentSpeed);
        
        if (result == CollisionResult.Continue && entityRigidbody != null)
        {
            entityRigidbody.linearVelocity = moveDirection * currentSpeed;
        }
        
        if (result == CollisionResult.Continue)
        {
            movementStrategy?.SetDirection(moveDirection);
            UpdateRotation();
        }
        
        HandleCollisionResult(result);
    }
    
    private void HandleCollisionResult(CollisionResult result)
    {
        switch (result)
        {
            case CollisionResult.Destroy:
                DestroyBullet();
                break;
            case CollisionResult.Stop:
                if (entityRigidbody != null)
                    entityRigidbody.linearVelocity = Vector2.zero;
                break;
            case CollisionResult.Continue:
                // 계속 진행
                break;
        }
    }
    
    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    
    protected virtual void DestroyBullet()
    {
        // 코루틴 정리
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        
        // Death Effect 실행
        deathEffectStrategy?.ExecuteEffect(transform.position, bulletConfig, owner);
        
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
        enemyCollisionStrategy = null;
        wallCollisionStrategy = null;
        deathEffectStrategy = null;
        
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
