using UnityEngine;
using System.Collections;

public abstract class LivingEntity : BaseEntity
{
    [Header("Health System")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    
    [Header("State")]
    protected bool facingRight = true;
    protected bool canMove = true;
    protected bool canJump = true;


    protected override void Awake()
    {
        base.Awake();
        currentHealth = maxHealth; // 체력 초기화
    }
    /** 체력 관련 **/
    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive) return;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{entityID} took {damage} damage. Health: {currentHealth}/{maxHealth}");
        
        // 피격 애니메이션 (하위 클래스에서 구현)
        OnTakeDamage(damage);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    public virtual void Heal(int amount)
    {
        if (!IsAlive) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Debug.Log($"{entityID} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }
    
    protected virtual void Die()
    {
        Debug.Log($"{entityID} died!");
        OnDie();
    }
    protected virtual void OnTakeDamage(int damage) { }
    protected virtual void OnDie() { }

    /** 기본 이동 **/
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
    public virtual void Attack() { }

    // 이동 제어 메서드들
    public void SetCanMove(bool canMove) { this.canMove = canMove; }
    public void SetCanJump(bool canJump) { this.canJump = canJump; }
    public bool GetFacingRight() => facingRight;
    
}
