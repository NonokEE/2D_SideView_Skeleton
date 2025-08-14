using UnityEngine;

public interface ICollisionAction
{
    CollisionActionResult Execute(CollisionContext context);
    int Priority { get; } // 실행 순서 (낮을수록 우선)
}

public struct CollisionContext
{
    public Bullet bullet;
    public Collider2D collider;
    public BaseEntity targetEntity; // null if obstacle
    public LayerMask layerMask;
    public Vector3 hitPoint;
    
    public bool IsEntityCollision => targetEntity != null;
    public bool IsObstacleCollision => targetEntity == null;
}

public struct CollisionActionResult
{
    public bool continueBullet;    // true: 탄환 계속, false: 정지
    public bool destroyBullet;     // true: 탄환 파괴
    public Vector2 newDirection;   // 방향 변경 (bounce용)
    public float speedMultiplier;  // 속도 배율 (damping용)
    
    // 기본값 제공을 위한 생성자
    public static CollisionActionResult Default => new()
    {
        continueBullet = true,
        destroyBullet = false,
        newDirection = Vector2.zero, // Vector2.zero면 방향 변경 없음
        speedMultiplier = 1f
    };
}
