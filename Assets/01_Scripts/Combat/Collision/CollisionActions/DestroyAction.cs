using UnityEngine;

public class DestroyAction : ICollisionAction
{
    public int Priority => 10; // 높은 우선순위 (마지막 실행)
    
    public CollisionActionResult Execute(CollisionContext context)
    {
        Debug.Log($"Bullet destroyed by collision with {context.collider.name}");
        
        return new CollisionActionResult
        {
            continueBullet = false,     // 탄환 진행 중단
            destroyBullet = true,       // 탄환 파괴
            newDirection = Vector2.zero,
            speedMultiplier = 0f
        };
    }
}
