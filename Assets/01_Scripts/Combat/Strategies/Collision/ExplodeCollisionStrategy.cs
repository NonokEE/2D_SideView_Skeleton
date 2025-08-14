using UnityEngine;

public class ExplodeCollisionStrategy : ICollisionStrategy
{
    public CollisionResult HandleEntityHit(BaseEntity target, ref int hitCount, ref int penetrationCount)
    {
        return CollisionResult.Destroy;
    }
    
    public CollisionResult HandleObstacleHit(Collider2D obstacle, ref int bounceCount, ref Vector2 moveDirection, ref float currentSpeed)
    {
        return CollisionResult.Destroy;
    }
}
