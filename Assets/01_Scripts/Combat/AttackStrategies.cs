// Assets/01_Scripts/Combat/AttackStrategies.cs
using UnityEngine;
using System.Collections.Generic;

// 즉시 피해 (탄환이나 근접 공격용)
// 접촉시 즉시 피해
public class InstantDamageStrategy : IAttackEnterStrategy
{
    public void OnAttackEnter(BaseEntity target, IDamageSource source)
    {
        DamageData damage = source.GenerateDamageData(target);
        target.TakeDamage((int)damage.damage);
        Debug.Log($"Instant damage: {damage.damage} to {target.entityID}");
    }
}

// 지속 피해 전략 (장판데미지)
// 공격 판정 안에 머무는 동안 주기적으로 피해
public class PeriodicDamageStrategy : IAttackStayStrategy
{
    private Dictionary<BaseEntity, float> lastDamageTime = new Dictionary<BaseEntity, float>();
    private float damageInterval = 1f; // 1초마다 피해

    public PeriodicDamageStrategy(float interval = 1f)
    {
        damageInterval = interval;
    }

    public void OnAttackStay(BaseEntity target, IDamageSource source)
    {
        if (!lastDamageTime.ContainsKey(target))
        {
            lastDamageTime[target] = 0f;
        }

        if (Time.time >= lastDamageTime[target] + damageInterval)
        {
            DamageData damage = source.GenerateDamageData(target);
            target.TakeDamage((int)(damage.damage * 0.5f)); // 지속 피해는 절반
            lastDamageTime[target] = Time.time;
            Debug.Log($"Periodic damage: {damage.damage * 0.5f} to {target.entityID}");
        }
    }
}

// 누적 피해 전략 (특수구역용)
public class AccumulatedDamageStrategy : IAttackEnterStrategy, IAttackStayStrategy, IAttackExitStrategy
{
    private Dictionary<BaseEntity, float> accumulatedDamage = new Dictionary<BaseEntity, float>();
    private Dictionary<BaseEntity, float> enterTime = new Dictionary<BaseEntity, float>();
    
    public void OnAttackEnter(BaseEntity target, IDamageSource source)
    {
        accumulatedDamage[target] = 100f; // 초기값 100
        enterTime[target] = Time.time;
        Debug.Log($"Accumulation started: {target.entityID}");
    }
    
    public void OnAttackStay(BaseEntity target, IDamageSource source)
    {
        if (accumulatedDamage.ContainsKey(target))
        {
            accumulatedDamage[target] -= 20f * Time.deltaTime; // 초당 20씩 감소
            accumulatedDamage[target] = Mathf.Max(0, accumulatedDamage[target]);
        }
    }
    
    public void OnAttackExit(BaseEntity target, IDamageSource source)
    {
        if (accumulatedDamage.ContainsKey(target))
        {
            float finalDamage = accumulatedDamage[target];
            if (finalDamage > 0)
            {
                target.TakeDamage((int)finalDamage);
                Debug.Log($"Final accumulated damage: {finalDamage} to {target.entityID}");
            }
            
            accumulatedDamage.Remove(target);
            enterTime.Remove(target);
        }
    }
}
