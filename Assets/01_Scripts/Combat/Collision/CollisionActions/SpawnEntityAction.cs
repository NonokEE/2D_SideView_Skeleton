using UnityEngine;

/// <summary>
/// Entity 생성 CollisionAction. 충돌 시 지정된 프리팹을 생성하여 다양한 효과 구현.
/// 현재 지원: ExplosionDamageSource, FieldDamageSource
/// 보류: BulletDamageSource (Split Bullet 시스템 - BulletPool 구현 후 재개 예정)
/// </summary>
public class SpawnEntityAction : ICollisionAction
{
    #region Constants
    
    private const string BULLET_DAMAGE_SOURCE_WARNING = "BulletDamageSource spawning is disabled (Split Bullet system postponed until BulletPool implementation)";
    private const int DEFAULT_PRIORITY = 5;
    
    #endregion
    
    #region Private Fields
    
    private readonly GameObject entityPrefab;
    private readonly int spawnCount;
    private readonly Vector2 spawnOffset;
    
    #endregion
    
    #region ICollisionAction Implementation
    
    public int Priority => DEFAULT_PRIORITY;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// SpawnEntityAction 생성자
    /// </summary>
    /// <param name="prefab">생성할 Entity 프리팹</param>
    /// <param name="count">생성할 개수 (기본: 1)</param>
    /// <param name="offset">생성 위치 오프셋 (기본: Vector2.zero)</param>
    public SpawnEntityAction(GameObject prefab, int count = 1, Vector2 offset = default)
    {
        entityPrefab = prefab;
        spawnCount = Mathf.Max(1, count); // 최소 1개 보장
        spawnOffset = offset;
    }
    
    #endregion
    
    #region Action Execution
    
    /// <summary>
    /// CollisionAction 실행. 지정된 개수만큼 Entity를 생성하고 초기화.
    /// </summary>
    public CollisionActionResult Execute(CollisionContext context)
    {
        if (!ValidateExecutionContext(context))
        {
            return CollisionActionResult.Default;
        }
        
        try
        {
            SpawnEntities(context);
        }
        catch (System.Exception ex)
        {
            HandleSpawnError(ex);
        }
        
        return CollisionActionResult.Default;
    }
    
    /// <summary>
    /// 실행 컨텍스트 유효성 검증
    /// </summary>
    private bool ValidateExecutionContext(CollisionContext context)
    {
        if (entityPrefab == null)
        {
            Debug.LogError("SpawnEntityAction: entityPrefab is null!");
            return false;
        }
        
        if (context.bullet == null)
        {
            Debug.LogError("SpawnEntityAction: context.bullet is null!");
            return false;
        }
        
        return true;
    }
    
    #endregion
    
    #region Entity Spawning
    
    /// <summary>
    /// 지정된 개수만큼 Entity 생성 및 초기화
    /// </summary>
    private void SpawnEntities(CollisionContext context)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = CalculateSpawnPosition(context.hitPoint, i);
            GameObject spawnedEntity = CreateEntityInstance(spawnPosition);
            
            if (spawnedEntity != null)
            {
                InitializeSpawnedEntity(spawnedEntity, context);
            }
        }
    }
    
    /// <summary>
    /// Entity 생성 위치 계산 (여러 개 생성 시 분산 배치)
    /// </summary>
    private Vector3 CalculateSpawnPosition(Vector3 basePosition, int index)
    {
        Vector3 spawnPosition = basePosition + (Vector3)spawnOffset;
        
        // 여러 개 생성 시 원형으로 분산 배치
        if (spawnCount > 1)
        {
            float angle = (360f / spawnCount) * index * Mathf.Deg2Rad;
            Vector2 scatterOffset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.3f;
            spawnPosition += (Vector3)scatterOffset;
        }
        
        return spawnPosition;
    }
    
    /// <summary>
    /// Entity 인스턴스 생성
    /// </summary>
    private GameObject CreateEntityInstance(Vector3 position)
    {
        GameObject spawned = Object.Instantiate(entityPrefab, position, Quaternion.identity);
        Debug.Log($"SpawnEntityAction: Created {spawned.name} at {position}");
        return spawned;
    }
    
    #endregion
    
    #region Entity Initialization
    
    /// <summary>
    /// 생성된 Entity 초기화 (타입별 분기 처리)
    /// </summary>
    private void InitializeSpawnedEntity(GameObject spawnedEntity, CollisionContext context)
    {
        if (TryInitializeBulletDamageSource(spawnedEntity))
        {
            // BulletDamageSource는 현재 보류 - 경고 메시지 출력 후 제거
            return;
        }
        
        if (TryInitializeOtherDamageSource(spawnedEntity, context))
        {
            // ExplosionDamageSource, FieldDamageSource 등 성공적 초기화
            return;
        }
        
        // DamageSourceEntity가 아닌 일반 GameObject
        Debug.LogWarning($"SpawnEntityAction: {spawnedEntity.name} does not have a supported DamageSourceEntity component");
    }
    
    /// <summary>
    /// BulletDamageSource 초기화 시도 (현재 보류된 기능)
    /// </summary>
    private bool TryInitializeBulletDamageSource(GameObject entity)
    {
        if (entity.TryGetComponent<BulletDamageSource>(out _))
        {
            Debug.LogWarning(BULLET_DAMAGE_SOURCE_WARNING);
            Object.Destroy(entity); // 생성된 객체 정리
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 기타 DamageSourceEntity 초기화 (ExplosionDamageSource, FieldDamageSource 등)
    /// </summary>
    private bool TryInitializeOtherDamageSource(GameObject entity, CollisionContext context)
    {
        if (entity.TryGetComponent<DamageSourceEntity>(out var damageSource) 
            && !(damageSource is BulletDamageSource))
        {
            InitializeDamageSource(damageSource, context);
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// DamageSourceEntity 초기화 실행
    /// </summary>
    private void InitializeDamageSource(DamageSourceEntity damageSource, CollisionContext context)
    {
        damageSource.Initialize(context.bullet.GetOwner(), context.bullet.GetWeapon());
        Debug.Log($"SpawnEntityAction: Initialized {damageSource.GetType().Name}");
    }
    
    #endregion
    
    #region Error Handling
    
    /// <summary>
    /// Entity 생성 중 발생한 오류 처리
    /// </summary>
    private void HandleSpawnError(System.Exception exception)
    {
        Debug.LogError($"SpawnEntityAction execution failed: {exception.Message}\nStackTrace: {exception.StackTrace}");
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// 현재 지원되는 Entity 타입 확인
    /// </summary>
    public static bool IsSupportedEntityType<T>() where T : DamageSourceEntity
    {
        // BulletDamageSource는 현재 지원되지 않음
        return typeof(T) != typeof(BulletDamageSource);
    }
    
    /// <summary>
    /// 생성 예정 Entity 정보
    /// </summary>
    public string GetSpawnInfo()
    {
        string prefabName = entityPrefab != null ? entityPrefab.name : "null";
        return $"Prefab: {prefabName}, Count: {spawnCount}, Offset: {spawnOffset}";
    }
    
    #endregion
}
