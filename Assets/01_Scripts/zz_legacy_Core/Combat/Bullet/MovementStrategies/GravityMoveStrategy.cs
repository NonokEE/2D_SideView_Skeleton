using UnityEngine;

public class GravityMovementStrategy : IMovementStrategy
{
    public bool RequiresUpdate => false;
    
    public void Initialize(Rigidbody2D rb, BulletPhysicsConfig config, Vector2 direction) { }
    public void UpdateMovement(float deltaTime, float moveTimer) { }
    public void SetDirection(Vector2 newDirection) { }
}
