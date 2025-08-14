using UnityEngine;

public class StopAction : ICollisionAction
{
    public int Priority => 3; // 중간 우선순위
    
    public CollisionActionResult Execute(CollisionContext context)
    {
        Debug.Log($"Bullet stopped by {context.collider.name}");
        
        return new CollisionActionResult
        {
            continueBullet = false,     // 탄환 정지
            destroyBullet = false,      // 파괴는 하지 않음 (정지만)
            newDirection = Vector2.zero,
            speedMultiplier = 0f        // 속도 0으로 설정
        };
    }
}
