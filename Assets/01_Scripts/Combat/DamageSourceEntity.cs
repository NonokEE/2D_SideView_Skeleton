// Assets/01_Scripts/Combat/DamageSourceEntity.cs
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageSourceEntity : BaseEntity, IDamageSource
{
    [Header("Damage Source")]
    [SerializeField] protected BaseEntity owner;
    [SerializeField] protected BaseWeapon weapon;
    [SerializeField] protected int damage = 10;
    
    [Header("Target Selection")]
    [SerializeField] protected LayerMask targetLayers = -1;        // 기본적으로 데미지 줄 레이어
    [SerializeField] protected List<BaseEntity> whitelist = new List<BaseEntity>(); // 강제 타겟
    [SerializeField] protected List<BaseEntity> blacklist = new List<BaseEntity>(); // 제외 타겟
    
    public virtual void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        owner = sourceOwner;
        weapon = sourceWeapon;
        if (weapon != null)
            damage = weapon.damage;
            
        // 기본적으로 소유자는 blacklist에 추가
        if (owner != null && !blacklist.Contains(owner))
            blacklist.Add(owner);
    }
    
    public virtual DamageData GenerateDamageData(BaseEntity target)
    {
        return new DamageData
        {
            attacker = owner,
            target = target,
            damageSource = this,
            damage = damage
        };
    }
    
    public BaseEntity GetOwner() => owner;
    public BaseWeapon GetWeapon() => weapon;
    
    public virtual bool CanDamage(BaseEntity target)
    {
        if (target == null || !target.IsAlive) return false;
        
        // Blacklist 체크 (최우선)
        if (blacklist.Contains(target)) return false;
        
        // Whitelist 체크 (LayerMask보다 우선)
        if (whitelist.Contains(target)) return true;
        
        // LayerMask 체크
        int targetLayer = target.gameObject.layer;
        return (targetLayers.value & (1 << targetLayer)) != 0;
    }
    
    // 피아식별 설정 메서드들
    public void AddToWhitelist(BaseEntity entity)
    {
        if (entity != null && !whitelist.Contains(entity))
            whitelist.Add(entity);
    }
    
    public void AddToBlacklist(BaseEntity entity)
    {
        if (entity != null && !blacklist.Contains(entity))
            blacklist.Add(entity);
    }
    
    public void SetTargetLayers(LayerMask layers)
    {
        targetLayers = layers;
    }
    
    public override void Initialize()
    {
        // BaseEntity 초기화는 별도 처리
    }
}
