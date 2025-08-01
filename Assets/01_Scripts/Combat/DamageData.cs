using UnityEngine;

[System.Serializable]
public struct DamageData
{
    [Header("Damage Info")]
    public BaseEntity attacker;        // 공격자
    public BaseWeapon weapon;          // 사용된 무기
    public BaseEntity target;          // 피격자
    public float damage;               // 피해량
    
    [Header("Additional Info")]
    public Vector2 attackDirection;    // 공격 방향 (넉백용)
    public float timestamp;            // 공격 발생 시간
    
    public DamageData(BaseEntity attacker, BaseWeapon weapon, BaseEntity target, float damage, Vector2 direction)
    {
        this.attacker = attacker;
        this.weapon = weapon;
        this.target = target;
        this.damage = damage;
        this.attackDirection = direction;
        this.timestamp = Time.time;
    }
}
