using UnityEngine;
using System.Collections;

public class Body_Wanderer : LivingEntity
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    public virtual void Move(float horizontal)
    {
        if (entityRigidbody == null) return;

        Vector2 velocity = entityRigidbody.linearVelocity;
        velocity.x = horizontal * moveSpeed;
        entityRigidbody.linearVelocity = velocity;

        if (horizontal > 0 && !facingRight)
            Flip();
        else if (horizontal < 0 && facingRight)
            Flip();
    }
    
    public virtual void Jump()
    {
        if (!IsGrounded || entityRigidbody == null) return;
        
        Vector2 velocity = entityRigidbody.linearVelocity;
        velocity.y = jumpForce;
        entityRigidbody.linearVelocity = velocity;
    }

    public virtual void Attack() { }
}
