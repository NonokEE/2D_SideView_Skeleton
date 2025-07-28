// Assets/01_Scripts/Player/PlayerEntity.cs
using UnityEngine;

public class PlayerEntity : LivingEntity
{

    private PlayerAnimationManager animationManager;
    public bool IsGrounded => isGrounded;

    protected override void Awake()
    {
        base.Awake();
        animationManager = GetComponent<PlayerAnimationManager>();
    }

    public override void Initialize()
    {
        // 플레이어 초기화 로직 (필요시 추가)
        Debug.Log($"Player {entityID} initialized");
    }
    
    // 나중에 구현할 기능들의 껍데기
    public override void Attack()
    {
        // TODO: 공격 시스템 구현 예정
        Debug.Log("Player Attack - Not implemented yet");
    }

    public override void Jump()
    {
        if (isGrounded)
        {
            // 점프 애니메이션 실행
            if (animationManager != null)
                animationManager.PlayJumpAnimation();
                
            entityRigidbody.linearVelocity = new Vector2(entityRigidbody.linearVelocity.x, jumpForce);
        }
    }
    
    public override void TakeDamage(int damage)
    {
        // 피격 애니메이션 실행
        if (animationManager != null)
            animationManager.PlayHitAnimation();
            
        // TODO: 실제 데미지 처리
        Debug.Log($"Player took {damage} damage");
    }
}
