using UnityEngine;

/// <summary>
/// 파괴 CollisionAction. 탄환을 즉시 파괴하여 게임에서 제거.
/// 가장 높은 우선순위를 가져 다른 액션들과 함께 실행되어도 최종적으로 탄환 파괴.
/// 
/// 알려진 이슈: maxExecutions가 1보다 큰 경우 동시 실행 문제 가능성 (3단계에서 해결 예정)
/// </summary>
public class DestroyAction : ICollisionAction
{
    #region Constants
    
    private const int DESTROY_PRIORITY = 10; // 가장 낮은 우선순위 (마지막 실행)
    
    #endregion
    
    #region ICollisionAction Implementation
    
    public int Priority => DESTROY_PRIORITY;
    
    #endregion
    
    #region Action Execution
    
    /// <summary>
    /// 파괴 액션 실행. 탄환을 즉시 파괴하고 모든 진행 중단.
    /// </summary>
    /// <param name="context">충돌 컨텍스트</param>
    /// <returns>탄환 파괴 결과</returns>
    public CollisionActionResult Execute(CollisionContext context)
    {
        LogDestroy(context);
        
        return new CollisionActionResult
        {
            continueBullet = false,     // 탄환 진행 중단
            destroyBullet = true,       // 탄환 파괴
            newDirection = Vector2.zero, // 방향 무의미
            speedMultiplier = 0f        // 속도 무의미
        };
    }
    
    #endregion
    
    #region Logging
    
    /// <summary>
    /// 파괴 발생 로그 출력
    /// </summary>
    private void LogDestroy(CollisionContext context)
    {
        string targetName = context.IsEntityCollision 
            ? context.targetEntity.entityID 
            : context.collider.name;
            
        Debug.Log($"Bullet destroyed by collision with {targetName}");
    }
    
    #endregion
}
