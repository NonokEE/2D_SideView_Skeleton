using UnityEngine;

public class SpawnEntityAction : ICollisionAction
{
    private readonly GameObject entityPrefab;
    private readonly int spawnCount;
    private readonly Vector2 spawnOffset;
    
    public int Priority => 5; // 중간 우선순위 (효과 실행용)
    
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
            Debug.LogWarning("SpawnEntityAction: entityPrefab is null!");
            return CollisionActionResult.Default;
        }
        
        // Entity 생성 (자탄, 폭발, 독구름 등)
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 spawnPos = context.hitPoint;
            
            // 여러 개 생성 시 위치 분산
            if (spawnCount > 1)
            {
                float angle = (360f / spawnCount) * i * Mathf.Deg2Rad;
                Vector2 scatter = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.5f;
                spawnPos += (Vector3)scatter;
            }
            
            spawnPos += (Vector3)spawnOffset;
            
            GameObject spawned = Object.Instantiate(entityPrefab, spawnPos, Quaternion.identity);
            
            // 소유자 정보 전달
            if (spawned.TryGetComponent<DamageSourceEntity>(out var damageSource))
            {
                damageSource.Initialize(context.bullet.GetOwner(), context.bullet.GetWeapon());
            }
        }
        
        Debug.Log($"SpawnEntityAction: Created {spawnCount} entities at {context.hitPoint}");
        
        return CollisionActionResult.Default; // 기본값 반환 (다른 액션에서 탄환 상태 결정)
    }
}
