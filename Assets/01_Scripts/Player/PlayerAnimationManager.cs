using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    private BaseEntity baseEntity;
    private VisualContainer visualContainer;
    private ScaleStretchAnimator jumpAnimator;
    private ColorBlinkAnimator hitAnimator;
    
    private void Start()
    {
        InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        baseEntity = GetComponent<BaseEntity>();
        if (baseEntity != null)
        {
            visualContainer = baseEntity.Visual;
            
            if (visualContainer != null)
            {
                jumpAnimator = visualContainer.GetGeometricAnimator<ScaleStretchAnimator>();
                hitAnimator = visualContainer.GetGeometricAnimator<ColorBlinkAnimator>();
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: VisualContainer not found!");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: BaseEntity component not found!");
        }
    }
    
    public void PlayJumpAnimation()
    {
        if (jumpAnimator != null && !jumpAnimator.IsPlaying)
        {
            jumpAnimator.PlayAnimation();
        }
    }
    
    public void PlayHitAnimation()
    {
        if (hitAnimator != null)
        {
            hitAnimator.PlayAnimation();
        }
    }
    
    public void StopAllAnimations()
    {
        if (visualContainer != null)
        {
            visualContainer.StopAllAnimations();
        }
    }
}
