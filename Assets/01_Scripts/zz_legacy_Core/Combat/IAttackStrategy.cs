using UnityEngine;

public interface IAttackEnterStrategy
{
    void OnAttackEnter(BaseEntity target, IDamageSource source);
}

public interface IAttackStayStrategy  
{
    void OnAttackStay(BaseEntity target, IDamageSource source);
}

public interface IAttackExitStrategy
{
    void OnAttackExit(BaseEntity target, IDamageSource source);
}
