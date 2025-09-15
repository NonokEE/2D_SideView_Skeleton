using UnityEngine;

/// <summary>
/// 정지 CollisionAction. 탄환을 즉시 정지시키지만 파괴하지는 않음.
/// 정지된 탄환은 다른 액션에 의해 다시 움직이거나 파괴될 수 있음.
/// </summary>
public class StopAction : ICollisionAction
{
    #region Constants
    
    private const int STOP_PRIORITY = 3; // 중간 우선순위
    
    #endregion
    
    #region ICollisionAction Implementation
    
    public int Priority => STOP_PRIORITY;
    
    #endregion
    
    #region Action Execution
    
    /// <summary>
    /// 정지 액션 실행. 탄환의 이동을 중단하되 GameObject는 유지.
    /// </summary>
    /// <param name="context">충돌 컨텍스트</param>
    /// <returns>탄환 정지 결과</returns>
    public CollisionActionResult Execute(CollisionContext context)
    {
        LogStop(context);
        
        return new CollisionActionResult
        {
            continueBullet = false,     // 탄환 진행 중단
            destroyBullet = false,      // 파괴는 하지 않음 (정지만)
            newDirection = Vector2.zero, // 방향 초기화
            speedMultiplier = 0f        // 속도 0으로 설정
        };
    }
    
    #endregion
    
    #region Logging
    
    /// <summary>
    /// 정지 발생 로그 출력
    /// </summary>
    private void LogStop(CollisionContext context)
    {
        string targetName = context.IsEntityCollision 
            ? context.targetEntity.entityID 
            : context.collider.name;
            
        Debug.Log($"Bullet stopped by {targetName}");
    }
    
    #endregion
}
