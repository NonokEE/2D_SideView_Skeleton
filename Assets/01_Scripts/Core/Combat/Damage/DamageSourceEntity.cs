using System.Collections.Generic;
using UnityEngine;

public abstract class DamageSourceEntity : BaseEntity, IDamageSource, IPoolable
{
    [Header("Damage Source")]
    [SerializeField] protected BaseEntity owner;
    [SerializeField] protected BaseWeapon weapon;
    [SerializeField] protected int damage = 10;

    [Header("Target Selection")]
    [SerializeField] protected LayerMask targetLayers = -1;              // 기본 타격 레이어
    [SerializeField] protected List<BaseEntity> whitelist = new();       // 강제 타겟
    [SerializeField] protected List<BaseEntity> blacklist = new();       // 제외 타겟

    public virtual void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        owner = sourceOwner;
        weapon = sourceWeapon;
        if (weapon != null) damage = weapon.damage;

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

        // Blacklist 우선
        if (blacklist.Contains(target)) return false;

        // Whitelist 우선
        if (whitelist.Contains(target)) return true;

        // LayerMask 검사
        int layer = target.gameObject.layer;
        return (targetLayers.value & (1 << layer)) != 0;
    }

    // 피아식별 설정
    public void AddToWhitelist(BaseEntity entity)
    {
        if (entity != null && !whitelist.Contains(entity)) whitelist.Add(entity);
    }

    public void AddToBlacklist(BaseEntity entity)
    {
        if (entity != null && !blacklist.Contains(entity)) blacklist.Add(entity);
    }

    public void SetTargetLayers(LayerMask layers) => targetLayers = layers;

    public override void Initialize()
    {
        // BaseEntity 초기화는 별도 처리
    }

    // ─── Pool 연동 공통 경로 ─────────────────────────────────────────────

    // 공통 파괴 경로: 풀 반환 시도 → 실패하면 폴백 비활성화
    protected virtual void DestroySelf()
    {
        // 코루틴/상태 정리는 파생 클래스 또는 OnDespawned/ResetForPool에서 수행
        if (PoolManager.Instance == null || !PoolManager.Instance.TryRelease(gameObject))
        {
            gameObject.SetActive(false);
        }
    }

    // IPoolable 기본 구현(필요 시 파생 클래스에서 override)
    public virtual void OnSpawned() { /* 스폰 직후 처리(파생 클래스에서 구현) */ }
    public virtual void OnDespawned() { /* 비활성 직전 처리(파생 클래스에서 구현) */ }
    public virtual void ResetForPool()
    {
        // 기본 피아식별 목록 정리(파생 클래스에서 Clear 호출 시점 통합)
        whitelist.Clear();
        blacklist.Clear();
    }
}
