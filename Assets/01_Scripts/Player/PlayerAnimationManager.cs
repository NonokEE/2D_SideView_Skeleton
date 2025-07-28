using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    [Header("Animation Components")]
    public ScaleStretchAnimator jumpAnimator;
    public ColorBlinkAnimator hitAnimator;
    public BaseGeometricAnimator idleAnimator;
    
    private PlayerEntity playerEntity;
    private Rigidbody2D playerRigidbody;
    
    // 애니메이션 상태
    private bool wasGrounded = true;
    private bool isJumping = false;
    
    private void Start()
    {
        playerEntity = GetComponent<PlayerEntity>();
        playerRigidbody = GetComponent<Rigidbody2D>();
        
        // 애니메이터들이 없으면 자동 추가
        if (jumpAnimator == null)
            jumpAnimator = gameObject.AddComponent<ScaleStretchAnimator>();
            
        if (hitAnimator == null)
            hitAnimator = gameObject.AddComponent<ColorBlinkAnimator>();
    }
    
    private void Update()
    {
        HandleMovementAnimations();
    }
    
    private void HandleMovementAnimations()
    {
        if (playerEntity == null) return;
        
        bool isGrounded = playerEntity.IsGrounded; // LivingEntity에서 접근 가능하도록 수정 필요
        bool isMoving = Mathf.Abs(playerRigidbody.linearVelocity.x) > 0.1f;
        bool isFalling = playerRigidbody.linearVelocity.y < -0.1f;
        
        // 점프 시작
        if (isGrounded && !wasGrounded && !isJumping)
        {
            PlayJumpAnimation();
            isJumping = true;
        }
        
        // 착지
        if (!isGrounded && wasGrounded && isJumping)
        {
            isJumping = false;
        }
        
        wasGrounded = isGrounded;
    }
    
    public void PlayJumpAnimation()
    {
        if (jumpAnimator != null && !jumpAnimator.IsPlaying)
        {
            jumpAnimator.PlayAnimation();
        }
    }
    
    public void PlayHitAnimation()
    {
        if (hitAnimator != null)
        {
            hitAnimator.PlayAnimation();
        }
    }
    
    public void PlayIdleAnimation()
    {
        if (idleAnimator != null && !idleAnimator.IsPlaying)
        {
            idleAnimator.PlayAnimation();
        }
    }
}
