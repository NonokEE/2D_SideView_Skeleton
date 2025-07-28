using UnityEngine;

public class PlayerEntity : LivingEntity
{
    [Header("Player Specific")]
    private PlayerAnimationManager animationManager;
    
    protected override void Awake()
    {
        base.Awake();
        animationManager = GetComponent<PlayerAnimationManager>();
    }
    
    public override void Initialize()
    {
        Debug.Log($"Player {entityID} initialized");
        
        // 기본 설정
        if (string.IsNullOrEmpty(entityID))
            entityID = "Player_01";
            
        // Rigidbody2D 기본 설정
        if (entityRigidbody != null)
        {
            entityRigidbody.bodyType = RigidbodyType2D.Dynamic;
            entityRigidbody.gravityScale = 1f;
            entityRigidbody.freezeRotation = true;
        }
    }
    
    public override void Jump()
    {
        if (!IsGrounded()) return;
        
        // 점프 애니메이션 실행
        if (animationManager != null)
            animationManager.PlayJumpAnimation();
            
        base.Jump();
    }
    
    public override void TakeDamage(int damage)
    {
        // 피격 애니메이션 실행
        if (animationManager != null)
            animationManager.PlayHitAnimation();
            
        Debug.Log($"Player took {damage} damage");
    }
    
    public override void Attack()
    {
        // TODO: 공격 시스템 구현 예정
        Debug.Log("Player Attack - Not implemented yet");
    }
    
    // 디버깅용 메서드
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // Inspector에서 설정 확인
        if (entityRigidbody == null)
            entityRigidbody = GetComponent<Rigidbody2D>();
    }
}
