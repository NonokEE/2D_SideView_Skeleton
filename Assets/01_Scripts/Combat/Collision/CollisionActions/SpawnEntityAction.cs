using UnityEngine;

/// <summary>
/// Entity 생성 CollisionAction. 충돌 시 지정된 프리팹을 생성하여 다양한 효과 구현.
/// 현재 지원: ExplosionDamageSource, FieldDamageSource
/// 보류: BulletDamageSource (Split Bullet 시스템 - BulletPool 구현 후 재개 예정)
/// </summary>
public class SpawnEntityAction : ICollisionAction
{
    #region Constants
    private const string BULLET_DAMAGE_SOURCE_WARNING =
        "BulletDamageSource spawning is disabled (Split Bullet system postponed until BulletPool implementation)";
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
    public SpawnEntityAction(GameObject prefab, int count = 1, Vector2 offset = default)
    {
        entityPrefab = prefab;
        spawnCount = Mathf.Max(1, count);
        spawnOffset = offset;
    }
    #endregion

    #region Action Execution
    public CollisionActionResult Execute(CollisionContext context)
    {
        if (!ValidateExecutionContext(context))
            return CollisionActionResult.Default;

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
    private void SpawnEntities(CollisionContext context)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPosition = CalculateSpawnPosition(context.hitPoint, i);
            GameObject spawnedEntity = CreateEntityInstance(spawnPosition);

            if (spawnedEntity != null)
                InitializeSpawnedEntity(spawnedEntity, context);
        }
    }

    private Vector3 CalculateSpawnPosition(Vector3 basePosition, int index)
    {
        Vector3 pos = basePosition + (Vector3)spawnOffset;

        if (spawnCount > 1)
        {
            float angle = (360f / spawnCount) * index * Mathf.Deg2Rad;
            Vector2 scatter = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.3f;
            pos += (Vector3)scatter;
        }

        return pos;
    }

    private GameObject CreateEntityInstance(Vector3 position)
    {
        GameObject spawned;
        if (PoolManager.Instance != null)
        {
            spawned = PoolManager.Instance.Spawn(entityPrefab, position, Quaternion.identity);
        }
        else
        {
            spawned = Object.Instantiate(entityPrefab, position, Quaternion.identity);
        }

        Debug.Log($"SpawnEntityAction: Created {spawned.name} at {position}");
        return spawned;
    }
    #endregion

    #region Entity Initialization
    private void InitializeSpawnedEntity(GameObject spawnedEntity, CollisionContext context)
    {
        // 자탄(보류 대상) 방지
        if (TryInitializeBulletDamageSource(spawnedEntity))
            return;

        // 폭발/필드 초기화
        if (TryInitializeOtherDamageSource(spawnedEntity, context))
            return;

        Debug.LogWarning($"SpawnEntityAction: {spawnedEntity.name} does not have a supported DamageSourceEntity component");
    }

    private bool TryInitializeBulletDamageSource(GameObject entity)
    {
        if (entity.TryGetComponent<BulletDamageSource>(out _))
        {
            Debug.LogWarning(BULLET_DAMAGE_SOURCE_WARNING);
            if (PoolManager.Instance != null)
                PoolManager.Instance.Release(entity);
            else
                Object.Destroy(entity);
            return true;
        }
        return false;
    }

    private bool TryInitializeOtherDamageSource(GameObject entity, CollisionContext context)
    {
        if (entity.TryGetComponent<DamageSourceEntity>(out var damageSource) &&
            !(damageSource is BulletDamageSource))
        {
            damageSource.Initialize(context.bullet.GetOwner(), context.bullet.GetWeapon());
            Debug.Log($"SpawnEntityAction: Initialized {damageSource.GetType().Name}");
            return true;
        }
        return false;
    }
    #endregion

    #region Error Handling
    private void HandleSpawnError(System.Exception exception)
    {
        Debug.LogError($"SpawnEntityAction execution failed: {exception.Message}\nStackTrace: {exception.StackTrace}");
    }
    #endregion

    #region Public Interface
    public static bool IsSupportedEntityType<T>() where T : DamageSourceEntity
        => typeof(T) != typeof(BulletDamageSource);

    public string GetSpawnInfo()
    {
        string prefabName = entityPrefab != null ? entityPrefab.name : "null";
        return $"Prefab: {prefabName}, Count: {spawnCount}, Offset: {spawnOffset}";
    }
    #endregion
}
