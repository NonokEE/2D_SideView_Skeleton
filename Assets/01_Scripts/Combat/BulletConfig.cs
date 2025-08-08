using UnityEngine;
using UnityEngine.Events;
using Combat.Projectiles;

[CreateAssetMenu(fileName = "New BulletConfig", menuName = "Combat/Bullet Config")]
public class BulletConfig : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string bulletName = "New Bullet";
    [TextArea(2, 4)]
    [SerializeField] private string description = "Bullet description...";

    [Header("Movement Properties")]
    [SerializeField] private MovementType movementType = MovementType.Straight;
    [SerializeField] private float initialSpeed = 10f;
    [SerializeField] private float maxSpeed = 25f;
    [SerializeField] private float accelerationTime = 0.5f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float rotationSpeed = 0f;
    
    [Header("Homing Properties")]
    [SerializeField] private float homingStrength = 0f;
    [SerializeField] private float homingRange = 5f;

    [Header("Physics Properties")]
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float mass = 0.1f;
    [SerializeField] private float drag = 0f;
    
    [Header("Collider Properties")]
    [SerializeField] private Vector2 colliderSize = new Vector2(0.2f, 0.2f);
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;
    [SerializeField] private BulletCapsuleDirection colliderDirection = BulletCapsuleDirection.Vertical;

    [Header("Collision Behavior")]
    [SerializeField] private HitBehavior enemyHitBehavior = HitBehavior.Stop;
    [SerializeField] private HitBehavior wallHitBehavior = HitBehavior.Stop;
    [SerializeField] private int maxPenetrations = 0;
    [SerializeField] private float bounceDamping = 0.8f;
    [SerializeField] private int maxBounces = 0;

    [Header("Lifetime Properties")]
    [SerializeField] private LifetimeType lifetimeType = LifetimeType.Time;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private int maxHits = 1;
    [SerializeField] private DeathEffectType deathEffect = DeathEffectType.None;

    [Header("Visual Properties")]
    [SerializeField] private Color bulletColor = Color.yellow;
    [SerializeField] private Vector3 bulletScale = new Vector3(0.2f, 0.2f, 1f);
    [SerializeField] private bool trailEffect = false;
    [SerializeField] private Color trailColor = Color.white;
    [SerializeField] private bool glowEffect = false;

    [Header("Gameplay Properties")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float criticalChance = 0f;
    [SerializeField] private float criticalMultiplier = 2f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Special Effects")]
    [SerializeField] private UnityEvent onSpawnEffect;
    [SerializeField] private UnityEvent onHitEnemyEffect;
    [SerializeField] private UnityEvent onHitWallEffect;
    [SerializeField] private UnityEvent onDeathEffect;

    // 프로퍼티들 (읽기 전용 접근)
    public string BulletName => bulletName;
    public string Description => description;
    
    // Movement Properties
    public MovementType MovementType => movementType;
    public float InitialSpeed => initialSpeed;
    public float MaxSpeed => maxSpeed;
    public float AccelerationTime => accelerationTime;
    public AnimationCurve SpeedCurve => speedCurve;
    public float RotationSpeed => rotationSpeed;
    public float HomingStrength => homingStrength;
    public float HomingRange => homingRange;
    
    // Physics Properties
    public bool UseGravity => useGravity;
    public float GravityScale => gravityScale;
    public float Mass => mass;
    public float Drag => drag;
    
    // Collider Properties
    public Vector2 ColliderSize => colliderSize;
    public Vector2 ColliderOffset => colliderOffset;
    public BulletCapsuleDirection ColliderDirection => colliderDirection;
    
    // Collision Behavior
    public HitBehavior EnemyHitBehavior => enemyHitBehavior;
    public HitBehavior WallHitBehavior => wallHitBehavior;
    public int MaxPenetrations => maxPenetrations;
    public float BounceDamping => bounceDamping;
    public int MaxBounces => maxBounces;
    
    // Lifetime Properties
    public LifetimeType LifetimeType => lifetimeType;
    public float MaxLifetime => maxLifetime;
    public float MaxDistance => maxDistance;
    public int MaxHits => maxHits;
    public DeathEffectType DeathEffect => deathEffect;
    
    // Visual Properties
    public Color BulletColor => bulletColor;
    public Vector3 BulletScale => bulletScale;
    public bool TrailEffect => trailEffect;
    public Color TrailColor => trailColor;
    public bool GlowEffect => glowEffect;
    
    // Gameplay Properties
    public float Damage => damage;
    public float CriticalChance => criticalChance;
    public float CriticalMultiplier => criticalMultiplier;
    public float KnockbackForce => knockbackForce;
    
    // Special Effects (읽기 전용으로 직접 접근)
    public UnityEvent OnSpawnEffect => onSpawnEffect;
    public UnityEvent OnHitEnemyEffect => onHitEnemyEffect;
    public UnityEvent OnHitWallEffect => onHitWallEffect;
    public UnityEvent OnDeathEffect => onDeathEffect;

    // 검증 메서드들
    private void OnValidate()
    {
        // 속도 값 검증
        initialSpeed = Mathf.Max(0f, initialSpeed);
        maxSpeed = Mathf.Max(initialSpeed, maxSpeed);
        accelerationTime = Mathf.Max(0.1f, accelerationTime);
        
        // 물리 값 검증
        mass = Mathf.Max(0.01f, mass);
        drag = Mathf.Max(0f, drag);
        
        // 충돌 값 검증
        maxPenetrations = Mathf.Max(0, maxPenetrations);
        maxBounces = Mathf.Max(0, maxBounces);
        bounceDamping = Mathf.Clamp01(bounceDamping);
        
        // 생명시간 값 검증
        maxLifetime = Mathf.Max(0.1f, maxLifetime);
        maxDistance = Mathf.Max(0.1f, maxDistance);
        maxHits = Mathf.Max(1, maxHits);
        
        // 게임플레이 값 검증
        damage = Mathf.Max(0f, damage);
        criticalChance = Mathf.Clamp01(criticalChance);
        criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
        knockbackForce = Mathf.Max(0f, knockbackForce);
    }
}
