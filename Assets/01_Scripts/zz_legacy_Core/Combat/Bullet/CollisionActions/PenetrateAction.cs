using UnityEngine;

/// <summary>
/// 관통 CollisionAction. 탄환이 충돌 대상을 관통하여 계속 진행하도록 함.
/// 주로 적 관통용으로 사용되며, maxExecutions로 관통 횟수 제한 가능.
/// </summary>
public class PenetrateAction : ICollisionAction
{
    #region Constants
    
    private const int PENETRATE_PRIORITY = 1; // 가장 높은 우선순위 (관통 판정 우선)
    
    #endregion
    
    #region ICollisionAction Implementation
    
    public int Priority => PENETRATE_PRIORITY;
    
    #endregion
    
    #region Action Execution
    
    /// <summary>
    /// 관통 액션 실행. 탄환이 대상을 관통하여 계속 진행하도록 설정.
    /// </summary>
    /// <param name="context">충돌 컨텍스트</param>
    /// <returns>탄환 계속 진행 결과</returns>
    public CollisionActionResult Execute(CollisionContext context)
    {
        LogPenetration(context);
        
        return new CollisionActionResult
        {
            continueBullet = true,      // 탄환 계속 진행
            destroyBullet = false,      // 파괴하지 않음
            newDirection = Vector2.zero, // 방향 변경 없음 (기존 방향 유지)
            speedMultiplier = 1f        // 속도 변화 없음
        };
    }
    
    #endregion
    
    #region Logging
    
    /// <summary>
    /// 관통 발생 로그 출력
    /// </summary>
    private void LogPenetration(CollisionContext context)
    {
        string targetName = context.IsEntityCollision 
            ? context.targetEntity.entityID 
            : context.collider.name;
            
        Debug.Log($"Bullet penetrated {targetName}");
    }
    
    #endregion
}
