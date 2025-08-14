using UnityEngine;

public class BounceAction : ICollisionAction
{
    private readonly float dampingFactor;
    
    public int Priority => 2; // 관통 이후 실행
    
    public BounceAction(float damping = 0.8f)
    {
        dampingFactor = damping;
    }
    
    public CollisionActionResult Execute(CollisionContext context)
    {
        // 반사 방향 계산
        Vector2 bulletPos = context.bullet.transform.position;
        Vector2 hitPoint = context.collider.ClosestPoint(bulletPos);
        Vector2 normal = (bulletPos - hitPoint).normalized;
        
        // 현재 이동 방향을 반사
        Vector2 currentDirection = context.bullet.GetDirection();
        Vector2 reflectedDirection = Vector2.Reflect(currentDirection, normal);
        
        Debug.Log($"Bullet bounced off {context.collider.name}, direction: {currentDirection} → {reflectedDirection}");
        
        return new CollisionActionResult
        {
            continueBullet = true,
            destroyBullet = false,
            newDirection = reflectedDirection,  // 반사된 방향으로 변경
            speedMultiplier = dampingFactor     // 속도 감쇠
        };
    }
}
