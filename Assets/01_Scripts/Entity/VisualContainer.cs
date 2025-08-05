using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class VisualContainer : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    
    [Header("Animation Managers")]
    [SerializeField] private BaseGeometricAnimator[] geometricAnimators;
    
    [Header("Invincibility Visual Effects")]
    private Coroutine blinkCoroutine;
    private bool isBlinking = false;
    
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
    
    private void Start()
    {
        // BaseEntity의 무적 이벤트 구독
        BaseEntity entity = GetComponentInParent<BaseEntity>();
        if (entity != null)
        {
            entity.OnInvincibilityStart += OnInvincibilityStart;
            entity.OnInvincibilityEnd += OnInvincibilityEnd;
        }
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
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }
    
    // 무적 이벤트 핸들러들
    private void OnInvincibilityStart(InvincibilityType type, float duration)
    {
        // 시각적 효과가 필요 없는 타입은 제외
        if (type == InvincibilityType.CutsceneInvincibility) return;
        
        Color effectColor = InvincibilityHelper.GetEffectColor(type);
        float blinkInterval = InvincibilityHelper.GetBlinkInterval(type);
        
        StartBlinkEffect(effectColor, blinkInterval);
    }
    
    private void OnInvincibilityEnd(InvincibilityType type)
    {
        // 더 이상 무적 상태가 없으면 깜빡임 중지
        BaseEntity entity = GetComponentInParent<BaseEntity>();
        if (entity != null && !entity.IsInvincible())
        {
            StopBlinkEffect();
        }
    }
    
    // 깜빡임 효과 관리
    public void StartBlinkEffect(Color blinkColor, float interval)
    {
        if (spriteRenderer == null) return;
        
        StopBlinkEffect();
        blinkCoroutine = StartCoroutine(BlinkCoroutine(blinkColor, interval));
    }
    
    public void StopBlinkEffect()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        
        // 원래 색상으로 복원
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        
        isBlinking = false;
    }
    
    private IEnumerator BlinkCoroutine(Color blinkColor, float interval)
    {
        isBlinking = true;
        
        while (isBlinking)
        {
            // 깜빡임 색상으로 변경
            if (spriteRenderer != null)
            {
                spriteRenderer.color = blinkColor;
            }
            
            yield return new WaitForSeconds(interval);
            
            // 원래 색상으로 복원
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
            
            yield return new WaitForSeconds(interval);
        }
    }
    
    // 기존 메서드들
    public void ResetToOriginalValues()
    {
        transform.localPosition = originalLocalPosition;
        transform.localScale = originalLocalScale;
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }
    
    public void SetColor(Color color)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
            // 깜빡임 중이 아닐 때만 원본 색상 업데이트
            if (!isBlinking)
                originalColor = color;
        }
    }
    
    public void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null)
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
    
    // 정리
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        BaseEntity entity = GetComponentInParent<BaseEntity>();
        if (entity != null)
        {
            entity.OnInvincibilityStart -= OnInvincibilityStart;
            entity.OnInvincibilityEnd -= OnInvincibilityEnd;
        }
        
        // 깜빡임 효과 정리
        StopBlinkEffect();
    }
}
