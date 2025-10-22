using UnityEngine;

public class BasicEnemyAnimationHandler : BaseAnimationHandler
{
    [Header("Basic Enemy Animation Indices")]
    private const int HIT_BLINK_INDEX = 0;          // ColorBlinkAnimator
    
    [Header("Basic Enemy Animation Settings")]
    [SerializeField] private bool enableHitAnimation = true;
    
    // 피격 효과 실행
    public void PlayHitEffect()
    {
        if (!enableHitAnimation) return;
        
        PlayAnimationAtIndex(HIT_BLINK_INDEX);
    }
}
