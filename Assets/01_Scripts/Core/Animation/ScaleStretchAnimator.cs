using UnityEngine;
using System.Collections;

public class ScaleStretchAnimator : BaseGeometricAnimator
{
    [Header("Stretch Settings")]
    public Vector2 stretchScale = new Vector2(0.8f, 1.3f);
    public float stretchDuration = 0.2f;
    public AnimationCurve stretchCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showAnimationGizmos = true;
    [SerializeField] private Color gizmoColor = Color.yellow;
    
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
    
    // ✅ 애니메이션 중 실시간 크기 시각화
    private void OnDrawGizmos()
    {
        if (!showAnimationGizmos || !Application.isPlaying) return;
        
        if (animTransform == null) return;
        
        // 현재 월드 스케일 계산
        Vector3 worldPosition = animTransform.position;
        Vector3 currentLocalScale = animTransform.localScale;
        
        // 부모의 스케일도 고려한 실제 월드 크기
        Vector3 worldScale = animTransform.lossyScale;
        
        // 애니메이션 중일 때는 더 진한 색상으로 표시
        Color currentColor = gizmoColor;
        if (isPlaying)
        {
            currentColor = Color.Lerp(gizmoColor, Color.red, 0.5f);
            currentColor.a = 0.8f;
        }
        else
        {
            currentColor.a = 0.3f;
        }
        
        Gizmos.color = currentColor;
        
        // 실제 크기로 와이어 박스 그리기
        Gizmos.DrawWireCube(worldPosition, worldScale);
        
        // 애니메이션 중일 때 추가 정보 표시
        if (isPlaying)
        {
            // 원본 크기도 함께 표시 (비교용)
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
            Vector3 originalWorldScale = Vector3.Scale(originalScale, animTransform.parent?.lossyScale ?? Vector3.one);
            Gizmos.DrawWireCube(worldPosition, originalWorldScale);
        }
    }
    
    // ✅ Scene View에서도 표시 (선택된 상태)
    private void OnDrawGizmosSelected()
    {
        if (!showAnimationGizmos) return;
        
        if (animTransform == null) animTransform = transform;
        
        Vector3 worldPosition = animTransform.position;
        
        // 최대 확대 크기 미리보기 (에디터에서)
        if (!Application.isPlaying)
        {
            Vector3 maxStretchScale = new Vector3(
                originalScale.x * stretchScale.x,
                originalScale.y * stretchScale.y,
                originalScale.z
            );
            
            Vector3 maxWorldScale = Vector3.Scale(maxStretchScale, animTransform.parent?.lossyScale ?? Vector3.one);
            
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.6f); // 오렌지색으로 최대 크기 표시
            Gizmos.DrawWireCube(worldPosition, maxWorldScale);
            
            // 원본 크기도 표시
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // 녹색으로 원본 크기 표시
            Vector3 originalWorldScale = Vector3.Scale(originalScale, animTransform.parent?.lossyScale ?? Vector3.one);
            Gizmos.DrawWireCube(worldPosition, originalWorldScale);
        }
    }
}
