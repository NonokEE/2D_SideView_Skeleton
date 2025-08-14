using UnityEngine;

public interface ICollisionStrategy
{
    CollisionResult HandleEntityHit(BaseEntity target, ref int hitCount, ref int penetrationCount);
    CollisionResult HandleObstacleHit(Collider2D obstacle, ref int bounceCount, ref Vector2 moveDirection, ref float currentSpeed);
}

public enum CollisionResult
{
    Continue,    // 계속 진행
    Stop,        // 탄환 정지
    Destroy      // 탄환 파괴
}
