using UnityEngine;

public interface IMovementStrategy
{
    void Initialize(Rigidbody2D rigidbody, BulletPhysicsConfig config, Vector2 direction);
    void UpdateMovement(float deltaTime, float moveTimer);
    void SetDirection(Vector2 newDirection);
    bool RequiresUpdate { get; }
}
