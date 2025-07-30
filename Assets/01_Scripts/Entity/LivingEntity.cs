using UnityEngine;
using System.Collections;

public abstract class LivingEntity : BaseEntity
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("State")]
    protected bool facingRight = true;
    protected bool canMove = true;
    protected bool canJump = true;
    
    public virtual void Move(float horizontal)
    {
        if (!canMove || entityRigidbody == null) return;
        
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
        if (!canJump || !IsGrounded() || entityRigidbody == null) return;
        
        Vector2 velocity = entityRigidbody.linearVelocity;
        velocity.y = jumpForce;
        entityRigidbody.linearVelocity = velocity;
    }
    
    protected virtual void Flip()
    {
        facingRight = !facingRight;

        if (visualContainer != null)
        {
            Vector3 scale = visualContainer.transform.localScale;
            scale.x *= -1;
            visualContainer.transform.localScale = scale;
        }
    }

    public virtual void DropThroughPlatform()
    {
        if (entityRigidbody == null) return;
        
        // 플랫폼 위에 서있을 때만 하강 가능
        if (IsGrounded() && IsOnPlatform())
        {
            StartCoroutine(TemporaryIgnorePlatform());
        }
    }

    private IEnumerator TemporaryIgnorePlatform()
    {
        Collider2D playerCollider = physicsContainer.MainCollider;
        playerCollider.enabled = false;
        
        yield return new WaitForSeconds(0.2f);
        
        playerCollider.enabled = true;
        
    }


    
    // 이동 제어 메서드들
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }
    
    public void SetCanJump(bool canJump)
    {
        this.canJump = canJump;
    }
    
    public bool GetFacingRight()
    {
        return facingRight;
    }
    
    // 추상 메서드들 (하위 클래스에서 구현)
    public virtual void Attack() { }
    public virtual void TakeDamage(int damage) { }
}
