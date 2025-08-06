using UnityEngine;
using System.Collections;

public class ScaleStretchAnimator : BaseGeometricAnimator
{
    [Header("Stretch Settings")]
    public Vector2 stretchScale = new Vector2(0.8f, 1.3f); // 가로 줄고 세로 늘어남
    public float stretchDuration = 0.2f;
    public AnimationCurve stretchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    protected override IEnumerator AnimationCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startScale = originalScale;
        Vector3 targetScale = new Vector3(
            originalScale.x * stretchScale.x,
            originalScale.y * stretchScale.y,
            originalScale.z
        );

        // 늘어나는 단계
        while (elapsedTime < stretchDuration)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            float progress = elapsedTime / stretchDuration;
            float curveValue = stretchCurve.Evaluate(progress);

            animTransform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            yield return null;
        }

        // 원래대로 돌아가는 단계
        elapsedTime = 0f;
        while (elapsedTime < stretchDuration)
        {
            elapsedTime += Time.deltaTime * animationSpeed;
            float progress = elapsedTime / stretchDuration;
            float curveValue = stretchCurve.Evaluate(progress);

            animTransform.localScale = Vector3.Lerp(targetScale, startScale, curveValue);
            yield return null;
        }

        ResetToOriginal();
        isPlaying = false;

        if (loop)
            PlayAnimation();
    }
    
    public override GeometricAnimationType GetAnimationType()
    {
        return GeometricAnimationType.ScaleStretch;
    }
}
