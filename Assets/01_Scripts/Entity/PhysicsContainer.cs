using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PhysicsContainer : MonoBehaviour
{
    [Header("Collision Components")]
    [SerializeField] private Collider2D mainCollider;
    [SerializeField] private Collider2D hitboxCollider;  // 공격 판정
    [SerializeField] private Collider2D hurtboxCollider; // 피격 판정
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float groundCheckRadius = 0.1f;
    
    // 프로퍼티들
    public Collider2D MainCollider => mainCollider;
    public Collider2D HitboxCollider => hitboxCollider;
    public Collider2D HurtboxCollider => hurtboxCollider;
    public Transform GroundCheckPoint => groundCheckPoint;
    
    private void Awake()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        if (mainCollider == null)
            mainCollider = GetComponent<Collider2D>();
    }
    
    public bool IsGrounded()
    {
        if (groundCheckPoint == null) return false;
        
        return Physics2D.OverlapCircle(
            groundCheckPoint.position, 
            groundCheckRadius, 
            groundLayer
        );
    }
    
    // Collider 제어 메서드들
    public void SetMainColliderEnabled(bool enabled)
    {
        if (mainCollider != null)
            mainCollider.enabled = enabled;
    }
    
    public void SetHitboxEnabled(bool enabled)
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = enabled;
    }
    
    public void SetHurtboxEnabled(bool enabled)
    {
        if (hurtboxCollider != null)
            hurtboxCollider.enabled = enabled;
    }
    
    // Gizmos로 Ground Check 시각화
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
