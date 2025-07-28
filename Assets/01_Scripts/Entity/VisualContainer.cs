using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class VisualContainer : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    [Header("Animation Managers")]
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
            geometricAnimators = GetComponents<BaseGeometricAnimator>();
    }
    
    private void StoreOriginalValues()
    {
        originalLocalPosition = transform.localPosition;
        originalLocalScale = transform.localScale;
        originalColor = spriteRenderer.color;
    }
    
    public void ResetToOriginalValues()
    {
        transform.localPosition = originalLocalPosition;
        transform.localScale = originalLocalScale;
        spriteRenderer.color = originalColor;
    }
    
    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }
    
    public void SetSprite(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
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
    
    public void StopAllAnimations()
    {
        if (geometricAnimators == null) return;
        
        foreach (var animator in geometricAnimators)
        {
            if (animator != null && animator.IsPlaying)
                animator.StopAnimation();
        }
    }
}
