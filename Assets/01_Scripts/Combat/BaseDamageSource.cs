// Assets/01_Scripts/Combat/BaseDamageSource.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class BaseDamageSource : MonoBehaviour, IDamageSource
{
    [Header("Damage Source")]
    protected BaseEntity attacker;
    protected BaseWeapon weapon;
    
    [Header("Attack Strategies")]
    protected IAttackEnterStrategy enterStrategy;
    protected IAttackStayStrategy stayStrategy;
    protected IAttackExitStrategy exitStrategy;
    
    [Header("Interaction Settings")]
    [SerializeField] protected LayerMask interactableLayers = ~0; // Default: interact with all layers
    [SerializeField] protected string[] includeEntityIDs = new string[0]; // IDs to force interaction
    [SerializeField] protected string[] excludeEntityIDs = new string[0]; // IDs to forbid interaction
    
    // 현재 영향받는 대상들 추적
    protected HashSet<BaseEntity> currentTargets = new HashSet<BaseEntity>();
    
    // 프로퍼티들
    public IAttackEnterStrategy EnterStrategy => enterStrategy;
    public IAttackStayStrategy StayStrategy => stayStrategy;
    public IAttackExitStrategy ExitStrategy => exitStrategy;

    public BaseEntity Attacker => attacker;
    public BaseWeapon Weapon => weapon;

    // IDamageSource 구현
    public BaseEntity GetAttacker() => attacker;
    public BaseWeapon GetWeapon() => weapon;
    public abstract DamageData GenerateDamageData(BaseEntity target);
    
    // 기본 초기화
    public virtual void Initialize(BaseEntity attacker, BaseWeapon weapon)
    {
        this.attacker = attacker;
        this.weapon = weapon;
        SetDefaultStrategies();
    }
    
    // 기본 전략 설정
    protected virtual void SetDefaultStrategies()
    {
        enterStrategy = null;
        stayStrategy = null;
        exitStrategy = null;
    }

    // 전략 설정 함수들
    public void SetEnterStrategy(IAttackEnterStrategy enter) { enterStrategy = enter; }
    public void SetStayStrategy(IAttackStayStrategy stay) { stayStrategy = stay; }
    public void SetExitStrategy(IAttackExitStrategy exit) { exitStrategy = exit; }

    protected bool IsTargetInteractable(BaseEntity target)
    {
        if (target == null) return false;

        // Check layer mask
        if ((interactableLayers.value & (1 << target.gameObject.layer)) == 0) return false;

        // Include list overrides (화이트리스트)
        if (includeEntityIDs != null && includeEntityIDs.Length > 0)
        {
            foreach (var id in includeEntityIDs)
            {
                if (id == target.entityID) return true;
            }
            return false; // Not in include list
        }
        
        // Exclude list (블랙리스트)
        if (excludeEntityIDs != null)
        {
            foreach (var id in excludeEntityIDs)
            {
                if (id == target.entityID) return false;
            }
        }

        return true;
    }

    // Virtual Execute 메서드들
    public virtual void ExecuteEnter(BaseEntity target)
    {
        if (!IsTargetInteractable(target)) return;

        if (target != null)
        {
            currentTargets.Add(target);
            enterStrategy?.OnAttackEnter(target, this);
        }
    }
    
    public virtual void ExecuteStay(BaseEntity target)
    {
        if (!IsTargetInteractable(target)) return;

        if (target != null && currentTargets.Contains(target))
        {
            stayStrategy?.OnAttackStay(target, this);
        }
    }
    
    public virtual void ExecuteExit(BaseEntity target)
    {
        if (!IsTargetInteractable(target)) return;

        if (target != null)
        {
            currentTargets.Remove(target);
            exitStrategy?.OnAttackExit(target, this);
        }
    }
    
    // 정리 작업
    protected virtual void OnDestroy()
    {
        foreach (BaseEntity target in currentTargets)
        {
            exitStrategy?.OnAttackExit(target, this);
        }
        currentTargets.Clear();
    }
}
