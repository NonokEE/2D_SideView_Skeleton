using UnityEngine;
using System.Collections.Generic;
using Combat.Projectiles;

[CreateAssetMenu(fileName = "New BulletPhysicsConfig", menuName = "Combat/Bullet Physics Config")]
public class BulletPhysicsConfig : ScriptableObject
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
    
    [Header("Movement Type Specific")]
    [SerializeField] private float sineAmplitude = 1f;
    [SerializeField] private float sineFrequency = 2f;
    [SerializeField] private float spiralRadius = 1f;
    [SerializeField] private float curveHeight = 2f;

    [Header("Homing Properties")]
    [SerializeField] private float homingStrength = 0f;
    [SerializeField] private float homingRange = 5f;

    [Header("Physics Properties")]
    [SerializeField] private bool useGravity = false;
    [SerializeField] private float gravityScale = 1f;
    [SerializeField] private float mass = 0.1f;
    [SerializeField] private float drag = 0f;

    [Header("Collider Properties")]
    [SerializeField] private Vector2 colliderSize = new Vector2(1.0f, 1.0f);
    [SerializeField] private Vector2 colliderOffset = Vector2.zero;
    [SerializeField] private BulletCapsuleDirection colliderDirection = BulletCapsuleDirection.Horizontal;

    [Header("Target Selection")]
    [SerializeField] private LayerMask targetLayers = -1;
    [SerializeField] private LayerMask obstacleLayers = -1;
    [SerializeField] private List<BaseEntity> whitelist = new List<BaseEntity>();
    [SerializeField] private List<BaseEntity> blacklist = new List<BaseEntity>();

    [Header("Collision Actions")]
    [SerializeField] private CollisionActionConfig[] enemyCollisionActions = new CollisionActionConfig[0];
    [SerializeField] private CollisionActionConfig[] wallCollisionActions = new CollisionActionConfig[0];

    [Header("Lifetime Properties")]
    [SerializeField] private LifetimeType lifetimeType = LifetimeType.Time;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private int maxHits = 1;
    [SerializeField] private DeathEffectType deathEffect = DeathEffectType.None;

    [Header("Gameplay Properties")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float criticalChance = 0f;
    [SerializeField] private float criticalMultiplier = 2f;
    [SerializeField] private float knockbackForce = 5f;

    [Header("Performance Properties")]
    [SerializeField] private bool enableUpdate = true;
    [SerializeField] private bool enableFixedUpdate = false;
    [SerializeField] private bool enableCollisionOptimization = true;
    [SerializeField] private bool poolingEnabled = true;
    [SerializeField] private int poolPreloadCount = 10;

    // CollisionActionConfig 구조체 정의
    [System.Serializable]
    public class CollisionActionConfig
    {
        [Header("Action Settings")]
        public CollisionActionType actionType = CollisionActionType.Stop;
        
        [Header("SpawnEntity Settings")]
        public GameObject entityPrefab;
        public int spawnCount = 1;
        public Vector2 spawnOffset = Vector2.zero;
        
        [Header("Bounce Settings")]
        [Range(0f, 1f)]
        public float dampingFactor = 0.8f;
        
        [Header("Execution Limits")]
        public int maxExecutions = 999; // 최대 실행 횟수
    }

    // 기존 프로퍼티들 (읽기 전용 접근)
    public string BulletName => bulletName;
    public string Description => description;
    
    // Movement Properties
    public MovementType MovementType => movementType;
    public float InitialSpeed => initialSpeed;
    public float MaxSpeed => maxSpeed;
    public float AccelerationTime => accelerationTime;
    public AnimationCurve SpeedCurve => speedCurve;
    public float RotationSpeed => rotationSpeed;
    
    // Movement Type Specific
    public float SineAmplitude => sineAmplitude;
    public float SineFrequency => sineFrequency;
    public float SpiralRadius => spiralRadius;
    public float CurveHeight => curveHeight;
    
    // Homing Properties
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
    
    // Target Selection
    public LayerMask TargetLayers => targetLayers;
    public LayerMask ObstacleLayers => obstacleLayers;
    public List<BaseEntity> Whitelist => whitelist;
    public List<BaseEntity> Blacklist => blacklist;
    
    // 새로운 Collision Actions 프로퍼티
    public CollisionActionConfig[] EnemyCollisionActions => enemyCollisionActions;
    public CollisionActionConfig[] WallCollisionActions => wallCollisionActions;
    
    // Lifetime Properties
    public LifetimeType LifetimeType => lifetimeType;
    public float MaxLifetime => maxLifetime;
    public float MaxDistance => maxDistance;
    public int MaxHits => maxHits;
    public DeathEffectType DeathEffect => deathEffect;
    
    // Gameplay Properties
    public float Damage => damage;
    public float CriticalChance => criticalChance;
    public float CriticalMultiplier => criticalMultiplier;
    public float KnockbackForce => knockbackForce;
    
    // Performance Properties
    public bool EnableUpdate => enableUpdate;
    public bool EnableFixedUpdate => enableFixedUpdate;
    public bool EnableCollisionOptimization => enableCollisionOptimization;
    public bool PoolingEnabled => poolingEnabled;
    public int PoolPreloadCount => poolPreloadCount;

    // 검증 메서드들 (기존과 동일, maxBounces/maxPenetrations 관련 제거)
    private void OnValidate()
    {
        // 속도 값 검증
        initialSpeed = Mathf.Max(0f, initialSpeed);
        maxSpeed = Mathf.Max(initialSpeed, maxSpeed);
        accelerationTime = Mathf.Max(0.1f, accelerationTime);
        
        // 물리 값 검증
        mass = Mathf.Max(0.01f, mass);
        drag = Mathf.Max(0f, drag);
        
        // 생명시간 값 검증
        maxLifetime = Mathf.Max(0.1f, maxLifetime);
        maxDistance = Mathf.Max(0.1f, maxDistance);
        maxHits = Mathf.Max(1, maxHits);
        
        // 게임플레이 값 검증
        damage = Mathf.Max(0f, damage);
        criticalChance = Mathf.Clamp01(criticalChance);
        criticalMultiplier = Mathf.Max(1f, criticalMultiplier);
        knockbackForce = Mathf.Max(0f, knockbackForce);
        
        // 성능 값 검증
        poolPreloadCount = Mathf.Max(1, poolPreloadCount);
        
        // Movement Type Specific 값 검증
        sineAmplitude = Mathf.Max(0f, sineAmplitude);
        sineFrequency = Mathf.Max(0.1f, sineFrequency);
        spiralRadius = Mathf.Max(0.1f, spiralRadius);
        curveHeight = Mathf.Max(0f, curveHeight);
        
        // CollisionAction 검증
        ValidateCollisionActions();
    }
    
    private void ValidateCollisionActions()
    {
        foreach (var action in enemyCollisionActions)
        {
            if (action.actionType == CollisionActionType.SpawnEntity && action.entityPrefab == null)
                Debug.LogWarning($"Enemy CollisionAction '{action.actionType}' requires entityPrefab");
        }
        
        foreach (var action in wallCollisionActions)
        {
            if (action.actionType == CollisionActionType.SpawnEntity && action.entityPrefab == null)
                Debug.LogWarning($"Wall CollisionAction '{action.actionType}' requires entityPrefab");
        }
    }
}
