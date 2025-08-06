using UnityEngine;
using System.Collections;

public class ColorBlinkAnimator : BaseGeometricAnimator
{
    [Header("Blink Settings")]
    public Color blinkColor = Color.red;
    public float blinkDuration = 0.1f;
    public int blinkCount = 3;

    protected override IEnumerator AnimationCoroutine()
    {
        if (!spriteRenderer) yield break;

        for (int i = 0; i < blinkCount; i++)
        {
            // 색상 변경
            spriteRenderer.color = blinkColor;
            yield return new WaitForSeconds(blinkDuration);

            // 원래 색상으로
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }

        isPlaying = false;

        if (loop)
            PlayAnimation();
    }
    
    public override GeometricAnimationType GetAnimationType()
    {
        return GeometricAnimationType.ColorBlink;
    }
}
