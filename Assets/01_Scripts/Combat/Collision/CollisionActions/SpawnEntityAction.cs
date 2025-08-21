using UnityEngine;

public class SpawnEntityAction : ICollisionAction
{
    private readonly GameObject entityPrefab;
    private readonly int spawnCount;
    private readonly Vector2 spawnOffset;
    
    public int Priority => 5; // 중간 우선순위

    public SpawnEntityAction(GameObject prefab, int count = 1, Vector2 offset = default)
    {
        entityPrefab = prefab;
        spawnCount = count;
        spawnOffset = offset;
    }

    public CollisionActionResult Execute(CollisionContext context)
    {
        if (entityPrefab == null)
        {
            Debug.LogError("SpawnEntityAction: entityPrefab is null!");
            return CollisionActionResult.Default;
        }

        try
        {
            for (int i = 0; i < spawnCount; i++)
            {
                Vector3 spawnPos = context.hitPoint + (Vector3)spawnOffset;
                
                // 여러 개 생성 시 위치 분산
                if (spawnCount > 1)
                {
                    float angle = (360f / spawnCount) * i * Mathf.Deg2Rad;
                    Vector2 scatter = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.3f;
                    spawnPos += (Vector3)scatter;
                }
                
                GameObject spawned = Object.Instantiate(entityPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"Successfully spawned: {spawned.name}");
                
                // ✅ BulletDamageSource 제외 - 자탄 시스템 보류됨
                // 현재는 ExplosionDamageSource, FieldDamageSource만 지원
                if (spawned.TryGetComponent<DamageSourceEntity>(out var damageSource) 
                    && !(damageSource is BulletDamageSource))
                {
                    Debug.Log($"Initializing {damageSource.GetType().Name}...");
                    damageSource.Initialize(context.bullet.GetOwner(), context.bullet.GetWeapon());
                }
                else if (damageSource is BulletDamageSource)
                {
                    Debug.LogWarning("BulletDamageSource spawning is disabled (Split Bullet system postponed)");
                    Object.Destroy(spawned); // 생성된 객체 정리
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SpawnEntityAction error: {ex.Message}\n{ex.StackTrace}");
        }
        
        return CollisionActionResult.Default;
    }
}
