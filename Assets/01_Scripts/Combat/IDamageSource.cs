using UnityEngine;

public interface IDamageSource
{
    // 전략들 (옵셔널)
    IAttackEnterStrategy EnterStrategy { get; }
    IAttackStayStrategy StayStrategy { get; }
    IAttackExitStrategy ExitStrategy { get; }
    
    // 기본 정보
    DamageData GenerateDamageData(BaseEntity target);
    BaseEntity GetAttacker();
    BaseWeapon GetWeapon();
    
    // 전략 실행 메서드들
    void ExecuteEnter(BaseEntity target);
    void ExecuteStay(BaseEntity target);
    void ExecuteExit(BaseEntity target);
}
