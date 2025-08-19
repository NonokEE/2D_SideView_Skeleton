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
            
            // ✅ BulletDamageSource 초기화 (SubBullet용)
            if (spawned.TryGetComponent<BulletDamageSource>(out var bullet))
            {
                // SubBullet용 Config 로딩
                BulletPhysicsConfig subBulletConfig = Resources.Load<BulletPhysicsConfig>("12_Configs/Combat/BulletConfigs/SubBulletConfig");
                
                if (subBulletConfig != null)
                {
                    bullet.Initialize(context.bullet.GetOwner(), context.bullet.GetWeapon(), subBulletConfig);
                    
                    // 자탄은 무작위 방향으로 발사
                    float randomAngle = Random.Range(-60f, 60f) + (i * (120f / spawnCount));
                    Vector2 randomDirection = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
                    
                    bullet.SetDirection(randomDirection);
                }
            }
            // ✅ 기타 DamageSourceEntity 초기화
            else if (spawned.TryGetComponent<DamageSourceEntity>(out var damageSource))
            {
                damageSource.Initialize(context.bullet.GetOwner(), context.bullet.GetWeapon());
            }
        }
        
        Debug.Log($"SpawnEntityAction: Created {spawnCount} entities at {context.hitPoint}");
        return CollisionActionResult.Default;
    }
}
