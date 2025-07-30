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
        // 플랫폼 충돌 임시 비활성화
        Physics2D.IgnoreLayerCollision(16, 7, true); // Player-Platform 충돌 무시

        if (entityRigidbody != null)
        {
            Vector2 velocity = entityRigidbody.linearVelocity;
            velocity.y = -8f; // 아래로 빠르게 이동
            entityRigidbody.linearVelocity = velocity;
        }

        Debug.Log("Ignoring platform collision for 0.5 seconds");
        yield return new WaitForSeconds(0.5f); // 0.5초 동안 무시
        Debug.Log("Re-enabling platform collision");

        // 충돌 다시 활성화
        Physics2D.IgnoreLayerCollision(16, 7, false);
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
