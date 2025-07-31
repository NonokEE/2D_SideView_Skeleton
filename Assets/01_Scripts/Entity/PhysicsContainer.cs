using UnityEngine;

public class PhysicsContainer : MonoBehaviour
{
    [Header("Collision Components")]
    [SerializeField] private Collider2D mainCollider;
    [SerializeField] private Collider2D hurtboxCollider;
    
    [Header("Collision Check Points")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Transform ceilingCheckPoint;
    
    [Header("Collision Settings")]
    [SerializeField] private LayerMask groundLayer = 1 << 6;
    [SerializeField] private LayerMask platformLayer = 1 << 7;
    [SerializeField] private LayerMask allSolidLayer = (1 << 6) | (1 << 7);
    [SerializeField] private Vector2 groundCheckSize = new Vector2(1.0f, 0.1f);    // 캐릭터와 같은 너비
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.9f);      // 캐릭터보다 살짝 짧게
    [SerializeField] private Vector2 ceilingCheckSize = new Vector2(1.0f, 0.1f);   // 캐릭터와 같은 너비
    
    // 프로퍼티들
    public Collider2D MainCollider => mainCollider;
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
    // Collider 제어 메서드들
    public void SetMainColliderEnabled(bool enabled)
    {
        if (mainCollider != null)
            mainCollider.enabled = enabled;
    }

    public void SetHurtboxEnabled(bool enabled)
    {
        if (hurtboxCollider != null)
            hurtboxCollider.enabled = enabled;
    }

    // 충돌 검사 메서드들
    public bool IsGrounded()
    {
        if (groundCheckPoint == null) return false;
        return Physics2D.OverlapBox(
            groundCheckPoint.position, 
            groundCheckSize, 
            0f, 
            allSolidLayer
        );
    }
    
    public bool IsOnPlatform()
    {
        if (groundCheckPoint == null) return false;
        // Platform 레이어만 체크
        return Physics2D.OverlapBox(
            groundCheckPoint.position, 
            groundCheckSize, 
            0f, 
            platformLayer
        );
    }
    
    public bool IsWallLeft()
    {
        if (wallCheckLeft == null) return false;
        return Physics2D.OverlapBox(
            wallCheckLeft.position,
            wallCheckSize,
            0f,
            groundLayer
        );
    }
    
    public bool IsWallRight()
    {
        if (wallCheckRight == null) return false;
        return Physics2D.OverlapBox(
            wallCheckRight.position, 
            wallCheckSize, 
            0f, 
            groundLayer
        );
    }
    
    public bool IsCeiling()
    {
        if (ceilingCheckPoint == null) return false;
        return Physics2D.OverlapBox(
            ceilingCheckPoint.position, 
            ceilingCheckSize, 
            0f, 
            groundLayer
        );
    }
    
    public bool IsWall(float direction)
    {
        if (direction > 0) return IsWallRight();
        else if (direction < 0) return IsWallLeft();
        return false;
    }
    
    
    // Gizmos로 모든 충돌 체크 지점 시각화
    private void OnDrawGizmosSelected()
    {
        // Ground Check
        if (groundCheckPoint != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
        
        // Wall Check Left
        if (wallCheckLeft != null)
        {
            Gizmos.color = IsWallLeft() ? Color.blue : Color.cyan;
            Gizmos.DrawWireCube(wallCheckLeft.position, wallCheckSize);
        }
        
        // Wall Check Right
        if (wallCheckRight != null)
        {
            Gizmos.color = IsWallRight() ? Color.blue : Color.cyan;
            Gizmos.DrawWireCube(wallCheckRight.position, wallCheckSize);
        }
        
        // Ceiling Check
        if (ceilingCheckPoint != null)
        {
            Gizmos.color = IsCeiling() ? Color.yellow : Color.white;
            Gizmos.DrawWireCube(ceilingCheckPoint.position, ceilingCheckSize);
        }
    }

    // 항상 표시되는 Gizmos (개발 중 편의)
    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green * 0.5f;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.blue * 0.5f;
            Gizmos.DrawWireCube(wallCheckLeft.position, wallCheckSize);
        }
        if (wallCheckRight != null)
        {
            Gizmos.color = Color.blue * 0.5f;
            Gizmos.DrawWireCube(wallCheckRight.position, wallCheckSize);
        }
        if (ceilingCheckPoint != null)
        {
            Gizmos.color = Color.yellow * 0.5f;
            Gizmos.DrawWireCube(ceilingCheckPoint.position, ceilingCheckSize);
        }
    }
}
