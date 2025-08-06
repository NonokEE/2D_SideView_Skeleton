using UnityEngine;

public class VisualContainer : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    [Header("Geometric Animators")]
    [SerializeField] private BaseGeometricAnimator[] geometricAnimators;
    
    private Vector3 originalLocalPosition;
    private Vector3 originalLocalScale;
    private Color originalColor;
    
    // 프로퍼티들
    public SpriteRenderer SpriteRenderer => spriteRenderer;
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
    
    // 인덱스 기반 애니메이터 관리
    public void PlayGeometricAnimation(int index)
    {
        if (IsValidAnimatorIndex(index))
        {
            geometricAnimators[index].PlayAnimation();
        }
    }
    
    public void StopGeometricAnimation(int index)
    {
        if (IsValidAnimatorIndex(index))
        {
            geometricAnimators[index].StopAnimation();
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
    
    // 타입별 접근
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
            originalColor = color;
        }
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = sprite;
    }
}
