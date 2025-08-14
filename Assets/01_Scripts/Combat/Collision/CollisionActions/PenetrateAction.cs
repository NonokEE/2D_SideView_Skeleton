using UnityEngine;

public class PenetrateAction : ICollisionAction
{
    public int Priority => 1; // 가장 먼저 실행 (관통 판정)
    
    public CollisionActionResult Execute(CollisionContext context)
    {
        Debug.Log($"Bullet penetrated {context.collider.name}");
        
        return new CollisionActionResult
        {
            continueBullet = true,      // 탄환 계속 진행
            destroyBullet = false,      // 파괴하지 않음
            newDirection = Vector2.zero, // 방향 변경 없음 (기존 방향 유지)
            speedMultiplier = 1f        // 속도 변화 없음
        };
    }
}
