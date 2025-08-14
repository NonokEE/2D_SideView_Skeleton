using UnityEngine;

public class BounceCollisionStrategy : ICollisionStrategy
{
    public CollisionResult HandleEntityHit(BaseEntity target, ref int hitCount, ref int penetrationCount)
    {
        return CollisionResult.Continue;
    }
    
    public CollisionResult HandleObstacleHit(Collider2D obstacle, ref int bounceCount, ref Vector2 moveDirection, ref float currentSpeed)
    {
        return CollisionResult.Continue;
    }
}
