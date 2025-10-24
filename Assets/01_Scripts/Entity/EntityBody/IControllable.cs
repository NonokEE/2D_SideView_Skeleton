using UnityEngine;

public interface IControllable
{
    public GameObject GameObject { get; }
    public Transform Transform { get; }
}

public interface IPlayerControllable : IControllable
{
    // TODO 플레이어 입력 핸들 정의 WHEN AFTER AI 구현 완료
    /* MovementHandle */
    void HandleMovementInput(Vector2 movementInput);
    
}
public interface IAIControllable : IControllable { }
