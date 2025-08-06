using UnityEngine;

public class VisualContainer : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    [Header("Geometric Animators")]
    [SerializeField] private BaseGeometricAnimator[] geometricAnimators;
    
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Color originalColor;
    
    // 프로퍼티들
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;
    public bool IsPlayingAnimation => geometricAnimators != null && System.Array.Exists(geometricAnimators, anim => anim != null && anim.IsPlaying);
    
    private void Awake()
    {
        InitializeComponents();
        StoreOriginalValues();
    }
    
    private void InitializeComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (geometricAnimators == null || geometricAnimators.Length == 0)
            geometricAnimators = GetComponentsInChildren<BaseGeometricAnimator>();
    }
    
    private void StoreOriginalValues()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }
    
    // 원본 값 복구
    public void ResetToOriginalValues()
    {
        transform.localPosition = originalLocalPosition;
        transform.localScale = originalLocalScale;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
    
    // 기본 시각적 설정
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
            originalColor = color; // 원본 색상 업데이트
        }
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
    }

    // GeometricAnimator 관리
    public void PlayGeometricAnimation(int index)
    {
        if (IsValidAnimatorIndex(index))
        {
            geometricAnimators[index].PlayAnimation();
            Debug.Log($"Playing geometric animation at index {index}: {geometricAnimators[index].GetType().Name}");
        }
        else
        {
            Debug.LogWarning($"Invalid animator index: {index}. Available range: 0-{geometricAnimators.Length - 1}");
        }
    }
    
    public void StopGeometricAnimation(int index)
    {
        if (IsValidAnimatorIndex(index))
        {
            geometricAnimators[index].StopAnimation();
            Debug.Log($"Stopped geometric animation at index {index}");
        }
    }
    
    public BaseGeometricAnimator GetGeometricAnimator(int index)
    {
        if (IsValidAnimatorIndex(index))
        {
            return geometricAnimators[index];
        }
        return null;
    }
    
    public int GetAnimatorCount()
    {
        return geometricAnimators?.Length ?? 0;
    }
    
    public bool IsAnimationPlaying(int index)
    {
        if (IsValidAnimatorIndex(index))
        {
            return geometricAnimators[index].IsPlaying;
        }
        return false;
    }
    
    public void StopAllAnimations()
    {
        if (geometricAnimators == null) return;
        
        foreach (var animator in geometricAnimators)
        {
            if (animator != null && animator.IsPlaying)
                animator.StopAnimation();
        }
    }
    
    private bool IsValidAnimatorIndex(int index)
    {
        return geometricAnimators != null && index >= 0 && index < geometricAnimators.Length;
    }
    
    public T GetGeometricAnimator<T>() where T : BaseGeometricAnimator
    {
        if (geometricAnimators == null) return null;
        
        foreach (var animator in geometricAnimators)
        {
            if (animator is T)
                return animator as T;
        }
        return null;
    }
    
    public BaseGeometricAnimator GetGeometricAnimatorByType(GeometricAnimationType animationType)
    {
        if (geometricAnimators == null) return null;
        
        foreach (var animator in geometricAnimators)
        {
            if (animator != null && GetAnimatorType(animator) == animationType)
                return animator;
        }
        return null;
    }
    
    public int GetAnimatorIndex<T>() where T : BaseGeometricAnimator
    {
        if (geometricAnimators == null) return -1;
        
        for (int i = 0; i < geometricAnimators.Length; i++)
        {
            if (geometricAnimators[i] is T)
                return i;
        }
        return -1;
    }
    
    public int GetAnimatorIndexByType(GeometricAnimationType animationType)
    {
        if (geometricAnimators == null) return -1;
        
        for (int i = 0; i < geometricAnimators.Length; i++)
        {
            if (geometricAnimators[i] != null && GetAnimatorType(geometricAnimators[i]) == animationType)
                return i;
        }
        return -1;
    }
    
    // 애니메이터 타입 판별 헬퍼 메서드
    private GeometricAnimationType GetAnimatorType(BaseGeometricAnimator animator)
    {
        switch (animator)
        {
            case ColorBlinkAnimator _:
                return GeometricAnimationType.ColorBlink;
            case ScaleStretchAnimator _:
                return GeometricAnimationType.ScaleStretch;
            // 추가 애니메이터 타입들...
            default:
                return GeometricAnimationType.ColorBlink; // 기본값
        }
    }
}
