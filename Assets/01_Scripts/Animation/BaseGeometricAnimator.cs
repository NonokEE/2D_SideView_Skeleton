using UnityEngine;
using System.Collections;

public abstract class BaseGeometricAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float animationSpeed = 1f;
    public bool playOnStart = false;
    public bool loop = false;

    [Header("References")]
    protected Transform animTransform;
    protected SpriteRenderer spriteRenderer;

    // 원본 값들 (애니메이션 복구용)
    protected Vector3 originalScale;
    protected Vector3 originalPosition;
    protected Color originalColor;
    protected float originalRotation;

    protected bool isPlaying = false;
    public bool IsPlaying => isPlaying;
    protected Coroutine currentAnimation;

    protected virtual void Awake()
    {
        animTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 원본 값 저장
        originalScale = animTransform.localScale;
        originalPosition = animTransform.localPosition;
        originalColor = spriteRenderer ? spriteRenderer.color : Color.white;
        originalRotation = animTransform.localEulerAngles.z;
    }

    protected virtual void Start()
    {
        if (playOnStart)
            PlayAnimation();
    }

    public virtual void PlayAnimation()
    {
        if (isPlaying) StopAnimation();

        isPlaying = true;
        currentAnimation = StartCoroutine(AnimationCoroutine());
    }

    public virtual void StopAnimation()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }

        isPlaying = false;
        ResetToOriginal();
    }

    protected virtual void ResetToOriginal()
    {
        animTransform.localScale = originalScale;
        animTransform.localPosition = originalPosition;
        animTransform.localEulerAngles = Vector3.forward * originalRotation;

        if (spriteRenderer)
            spriteRenderer.color = originalColor;
    }

    protected abstract IEnumerator AnimationCoroutine();
    public abstract GeometricAnimationType GetAnimationType();
}
