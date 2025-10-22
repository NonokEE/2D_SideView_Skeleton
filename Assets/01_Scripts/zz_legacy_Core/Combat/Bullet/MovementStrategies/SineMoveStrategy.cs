using UnityEngine;

public class SineMovementStrategy : IMovementStrategy
{
    public bool RequiresUpdate => true;
    
    public void Initialize(Rigidbody2D rb, BulletPhysicsConfig config, Vector2 direction) { }
    public void UpdateMovement(float deltaTime, float moveTimer) { }
    public void SetDirection(Vector2 newDirection) { }
}
