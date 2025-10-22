using UnityEngine;

public abstract class BaseAnimationHandler : MonoBehaviour
{
    [Header("Animation Handler")]
    protected BaseEntity baseEntity;
    protected VisualContainer visualContainer;
    
    [Header("Debug Info")]
    [SerializeField] protected bool showDebugLogs = true;
    
    protected virtual void Start()
    {
        InitializeComponents();
    }
    
    protected virtual void InitializeComponents()
    {
        baseEntity = GetComponent<BaseEntity>();
        if (baseEntity != null)
        {
            visualContainer = baseEntity.Visual;
            
            if (visualContainer == null)
            {
                Debug.LogError($"{gameObject.name}: VisualContainer not found in BaseEntity!");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: BaseEntity component not found!");
        }
    }
    
    // 공통 애니메이션 제어 메서드들
    protected bool PlayAnimationAtIndex(int index)
    {
        if (visualContainer == null) return false;
        
        if (visualContainer.IsAnimationPlaying(index))
        {
            if (showDebugLogs)
                Debug.Log($"{gameObject.name}: Animation at index {index} already playing, stopping first");
            visualContainer.StopGeometricAnimation(index);
        }
        
        visualContainer.PlayGeometricAnimation(index);
        
        if (showDebugLogs)
            Debug.Log($"{gameObject.name}: Playing animation at index {index}");
        
        return true;
    }
    
    protected bool StopAnimationAtIndex(int index)
    {
        if (visualContainer == null) return false;
        
        visualContainer.StopGeometricAnimation(index);
        
        if (showDebugLogs)
            Debug.Log($"{gameObject.name}: Stopped animation at index {index}");
        
        return true;
    }
    
    protected bool IsAnimationPlayingAtIndex(int index)
    {
        return visualContainer?.IsAnimationPlaying(index) ?? false;
    }
    
    // 모든 애니메이션 중지
    public virtual void StopAllAnimations()
    {
        if (visualContainer != null)
        {
            visualContainer.StopAllAnimations();
            if (showDebugLogs)
                Debug.Log($"{gameObject.name}: All animations stopped");
        }
    }
}
