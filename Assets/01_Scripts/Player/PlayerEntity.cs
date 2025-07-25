// Assets/01_Scripts/Player/PlayerEntity.cs
using UnityEngine;

public class PlayerEntity : LivingEntity
{
    public override void Initialize()
    {
        // 플레이어 초기화 로직 (필요시 추가)
        Debug.Log($"Player {entityID} initialized");
    }
    
    // 나중에 구현할 기능들의 껍데기
    public override void Attack()
    {
        // TODO: 공격 시스템 구현 예정
        Debug.Log("Player Attack - Not implemented yet");
    }
    
    public override void TakeDamage(int damage)
    {
        // TODO: 데미지 시스템 구현 예정
        Debug.Log($"Player took {damage} damage - Not implemented yet");
    }
}
