using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 50f;
    
    [Header("Jump")]
    public float jumpForce = 15f;
    public int maxJumpCount = 2;
    public float jumpCutMultiplier = 0.5f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayerMask;
    
    [Header("Slope")]
    public float maxSlopeAngle = 45f;
    
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    private bool isJumping;
    private RaycastHit2D slopeHit;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Ground Check 오브젝트 생성
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = groundCheckObj.transform;
        }
    }
    
    void Update()
    {
        CheckGrounded();
        HandleInput();
    }
    
    void FixedUpdate()
    {
        HandleMovement();
        HandleSlope();
    }
    
    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayerMask);
        
        if (!wasGrounded && isGrounded)
        {
            jumpCount = 0; // 착지 시 점프 카운트 리셋
        }
    }
    
    void HandleInput()
    {
        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (jumpCount < maxJumpCount)
            {
                Jump();
            }
        }
        
        // 점프 컷 (짧은 점프)
        if (Input.GetKeyUp(KeyCode.Space) && rb.linearVelocityY > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, rb.linearVelocityY * jumpCutMultiplier);
        }
    }
    
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float targetSpeed = horizontalInput * moveSpeed;
        
        // 경사면에서의 속도 조정
        if (OnSlope() && !isJumping)
        {
            targetSpeed = horizontalInput * moveSpeed * (1f - Mathf.Abs(GetSlopeAngle()) / 90f);
        }
        
        float currentSpeed = rb.linearVelocityX;
        float speedDifference = targetSpeed - currentSpeed;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        
        float movement = speedDifference * accelRate * Time.fixedDeltaTime;
        rb.AddForce(Vector2.right * movement, ForceMode2D.Force);
    }
    
    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpForce);
        jumpCount++;
        isJumping = true;
        
        // 점프 후 잠시 후 isJumping 해제
        Invoke(nameof(ResetJump), 0.1f);
    }
    
    void ResetJump()
    {
        isJumping = false;
    }
    
    bool OnSlope()
    {
        slopeHit = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayerMask);
        
        if (slopeHit.collider != null)
        {
            float angle = Vector2.Angle(Vector2.up, slopeHit.normal);
            return angle > 0 && angle <= maxSlopeAngle;
        }
        return false;
    }
    
    float GetSlopeAngle()
    {
        return Vector2.Angle(Vector2.up, slopeHit.normal);
    }
    
    void HandleSlope()
    {
        if (OnSlope() && !isJumping && isGrounded)
        {
            // 경사면에서 미끄러지지 않도록 중력 상쇄
            rb.AddForce(-Physics2D.gravity * rb.gravityScale * 0.3f, ForceMode2D.Force);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        // 경사면 감지 레이 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down);
    }
}