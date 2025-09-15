using UnityEngine;

public class PlayerAnimationHandler : BaseAnimationHandler
{
    [Header("Player Animation Indices")]
    private const int JUMP_ANIMATOR_INDEX = 0;      // ScaleStretchAnimator
    private const int HIT_ANIMATOR_INDEX = 1;       // ColorBlinkAnimator
    
    [Header("Player Animation Settings")]
    [SerializeField] private bool enableJumpAnimation = true;
    [SerializeField] private bool enableHitAnimation = true;
    
    // 점프 애니메이션 실행
    public void PlayJumpAnimation()
    {
        if (!enableJumpAnimation) return;
        
        PlayAnimationAtIndex(JUMP_ANIMATOR_INDEX);
    }
    
    // 피격 애니메이션 실행 (단발성)
    public void PlayHitAnimation()
    {
        if (!enableHitAnimation) return;
        
        PlayAnimationAtIndex(HIT_ANIMATOR_INDEX);
    }
}
