using UnityEngine;
using System.Collections;

public class ColorBlinkAnimator : BaseGeometricAnimator
{
    [Header("Blink Settings")]
    public Color blinkColor = Color.red;
    public float blinkTotalDuration = 0.6f;    // 전체 점멸 지속시간
    public float blinkInterval = 0.1f;         // 점멸 주기 (색상 변경 간격)
    protected override IEnumerator AnimationCoroutine()
    {
        if (!spriteRenderer) yield break;
        
        float elapsed = 0f;
        bool isBlinkColor = false; // 현재 점멸 색상인지 여부
        
        // 지속시간 동안 점멸 반복
        while (elapsed < blinkTotalDuration)
        {
            // 색상 토글
            spriteRenderer.color = isBlinkColor ? blinkColor : originalColor;
            isBlinkColor = !isBlinkColor;
            
            // 점멸 간격만큼 대기
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        
        // 애니메이션 종료 시 원본 색상으로 복귀
        spriteRenderer.color = originalColor;
        isPlaying = false;
        
        if (loop)
            PlayAnimation();
    }
    
    public override GeometricAnimationType GetAnimationType()
    {
        return GeometricAnimationType.ColorBlink;
    }
}
