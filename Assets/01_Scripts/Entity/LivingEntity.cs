// Assets/01_Scripts/Entity/LivingEntity.cs
using UnityEngine;

public abstract class LivingEntity : BaseEntity
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("Ground Check")]
    public LayerMask groundLayer = 1;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    
    [Header("State")]
    protected bool isGrounded;
    protected bool facingRight = true;
    
    // Controller에서 호출할 메서드들
    public virtual void Move(float horizontal)
    {
        Vector2 velocity = entityRigidbody.linearVelocity;
        velocity.x = horizontal * moveSpeed;
        entityRigidbody.linearVelocity = velocity;
        
        // 방향 전환
        if (horizontal > 0 && !facingRight)
            Flip();
        else if (horizontal < 0 && facingRight)
            Flip();
    }
    
    public virtual void Jump()
    {
        if (isGrounded)
        {
            entityRigidbody.linearVelocity = new Vector2(entityRigidbody.linearVelocity.x, jumpForce);
        }
    }
    
    protected virtual void Update()
    {
        CheckGrounded();
    }
    
    protected virtual void CheckGrounded()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
    }
    
    protected virtual void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = entityTransform.localScale;
        scale.x *= -1;
        entityTransform.localScale = scale;
    }
    
    // 아직 구현하지 않을 추상 메서드들 (껍데기)
    public virtual void Attack() { /* TODO: 나중에 구현 */ }
    public virtual void TakeDamage(int damage) { /* TODO: 나중에 구현 */ }
}
