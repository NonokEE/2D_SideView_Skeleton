using UnityEngine;

/// <summary>
/// 반사 CollisionAction. 탄환이 충돌 대상에서 반사되어 방향을 바꿈.
/// 물리적인 반사 계산과 속도 감쇠 적용. maxExecutions로 반사 횟수 제한 가능.
/// 
/// 알려진 이슈: 현재 Destroy Action과 동시 실행 시 우선순위 문제 있음 (3단계에서 해결 예정)
/// </summary>
public class BounceAction : ICollisionAction
{
    #region Constants
    
    private const int BOUNCE_PRIORITY = 2; // 관통 이후 실행
    private const float DEFAULT_DAMPING = 0.8f;
    private const float MIN_DAMPING = 0.1f;
    private const float MAX_DAMPING = 1.0f;
    
    #endregion
    
    #region Private Fields
    
    private readonly float dampingFactor;
    
    #endregion
    
    #region ICollisionAction Implementation
    
    public int Priority => BOUNCE_PRIORITY;
    
    #endregion
    
    #region Constructor
    
    /// <summary>
    /// BounceAction 생성자
    /// </summary>
    /// <param name="damping">속도 감쇠 계수 (0.1 ~ 1.0, 기본값: 0.8)</param>
    public BounceAction(float damping = DEFAULT_DAMPING)
    {
        dampingFactor = Mathf.Clamp(damping, MIN_DAMPING, MAX_DAMPING);
    }
    
    #endregion
    
    #region Action Execution
    
    /// <summary>
    /// 반사 액션 실행. 충돌 법선 벡터를 기준으로 탄환 방향 반사.
    /// </summary>
    /// <param name="context">충돌 컨텍스트</param>
    /// <returns>반사된 방향과 감쇠된 속도</returns>
    public CollisionActionResult Execute(CollisionContext context)
    {
        if (!ValidateContext(context))
        {
            return GetFailureResult();
        }
        
        Vector2 reflectedDirection = CalculateReflectedDirection(context);
        LogBounce(context, reflectedDirection);
        
        return new CollisionActionResult
        {
            continueBullet = true,
            destroyBullet = false,
            newDirection = reflectedDirection,
            speedMultiplier = dampingFactor
        };
    }
    
    #endregion
    
    #region Reflection Calculation
    
    /// <summary>
    /// 컨텍스트 유효성 검증
    /// </summary>
    private bool ValidateContext(CollisionContext context)
    {
        if (context.bullet == null)
        {
            Debug.LogError("BounceAction: context.bullet is null!");
            return false;
        }
        
        if (context.collider == null)
        {
            Debug.LogError("BounceAction: context.collider is null!");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 반사 방향 계산
    /// </summary>
    private Vector2 CalculateReflectedDirection(CollisionContext context)
    {
        Vector2 bulletPosition = context.bullet.transform.position;
        Vector2 closestPoint = context.collider.ClosestPoint(bulletPosition);
        Vector2 normal = (bulletPosition - closestPoint).normalized;
        
        // 법선 벡터가 유효하지 않은 경우 기본값 사용
        if (normal.magnitude < 0.1f)
        {
            normal = Vector2.up; // 기본 법선 벡터
            Debug.LogWarning("BounceAction: Invalid normal vector, using default up vector");
        }
        
        Vector2 currentDirection = context.bullet.GetDirection();
        Vector2 reflectedDirection = Vector2.Reflect(currentDirection, normal);
        
        return reflectedDirection.normalized;
    }
    
    /// <summary>
    /// 실패 시 기본 결과 반환
    /// </summary>
    private CollisionActionResult GetFailureResult()
    {
        return new CollisionActionResult
        {
            continueBullet = false,
            destroyBullet = true, // 실패 시 탄환 파괴
            newDirection = Vector2.zero,
            speedMultiplier = 0f
        };
    }
    
    #endregion
    
    #region Logging
    
    /// <summary>
    /// 반사 발생 로그 출력
    /// </summary>
    private void LogBounce(CollisionContext context, Vector2 reflectedDirection)
    {
        Vector2 originalDirection = context.bullet.GetDirection();
        string targetName = context.IsEntityCollision 
            ? context.targetEntity.entityID 
            : context.collider.name;
            
        Debug.Log($"Bullet bounced off {targetName}, direction: {originalDirection} → {reflectedDirection}");
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// 현재 감쇠 계수 확인
    /// </summary>
    public float GetDampingFactor() => dampingFactor;
    
    #endregion
}
