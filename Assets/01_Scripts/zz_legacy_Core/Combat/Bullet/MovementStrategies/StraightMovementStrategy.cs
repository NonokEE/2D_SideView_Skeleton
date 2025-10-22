using UnityEngine;

public class StraightMovementStrategy : IMovementStrategy
{
    private Rigidbody2D rigidbody;
    private Vector2 moveDirection;
    private float currentSpeed;
    private float initialSpeed;
    private float maxSpeed;
    private float accelerationTime;
    private AnimationCurve speedCurve;

    public bool RequiresUpdate => accelerationTime > 0;

    public void Initialize(Rigidbody2D rb, BulletPhysicsConfig config, Vector2 direction)
    {
        rigidbody = rb;
        moveDirection = direction.normalized;
        initialSpeed = config.InitialSpeed;
        maxSpeed = config.MaxSpeed;
        accelerationTime = config.AccelerationTime;
        speedCurve = config.SpeedCurve;
        currentSpeed = initialSpeed;
        
        UpdateVelocity();
    }

    public void UpdateMovement(float deltaTime, float moveTimer)
    {
        if (accelerationTime > 0)
        {
            float speedProgress = Mathf.Clamp01(moveTimer / accelerationTime);
            float curveValue = speedCurve.Evaluate(speedProgress);
            currentSpeed = Mathf.Lerp(initialSpeed, maxSpeed, curveValue);
            UpdateVelocity();
        }
    }

    public void SetDirection(Vector2 newDirection)
    {
        moveDirection = newDirection.normalized;
        UpdateVelocity();
    }

    private void UpdateVelocity()
    {
        if (rigidbody != null)
            rigidbody.linearVelocity = moveDirection * currentSpeed;
    }
}
