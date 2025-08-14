using UnityEngine;

public class HomingMovementStrategy : IMovementStrategy
{
    private Rigidbody2D rigidbody;
    private Vector2 moveDirection;
    private float currentSpeed;
    private float homingStrength;
    private float homingRange;
    private LayerMask targetLayers;
    private BaseEntity homingTarget;
    
    public bool RequiresUpdate => true;

    public void Initialize(Rigidbody2D rb, BulletPhysicsConfig config, Vector2 direction)
    {
        rigidbody = rb;
        moveDirection = direction.normalized;
        currentSpeed = config.InitialSpeed;
        homingStrength = config.HomingStrength;
        homingRange = config.HomingRange;
        targetLayers = config.TargetLayers;
    }

    public void UpdateMovement(float deltaTime, float moveTimer)
    {
        if (homingTarget == null || !homingTarget.IsAlive)
            FindHomingTarget();
            
        if (homingTarget != null)
        {
            Vector2 targetDirection = (homingTarget.transform.position - rigidbody.transform.position).normalized;
            moveDirection = Vector2.Lerp(moveDirection, targetDirection, homingStrength * deltaTime);
            moveDirection.Normalize();
        }
        
        rigidbody.linearVelocity = moveDirection * currentSpeed;
    }

    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
    }
    
    private void FindHomingTarget()
    {
        // ... 유도 타겟 찾기 로직
    }
}
