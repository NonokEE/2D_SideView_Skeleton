using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 엔티티의 생명에 관한 스키마를 담당. 체력, 무적, 죽음 등
/// </summary>
public abstract class LivingEntity : BaseEntity
{
    [Header("Health System")]
    [SerializeField] protected int maxHealth = 100;
    public int MaxHealth => maxHealth;

    [SerializeField] protected int currentHealth;
    public int CurrentHealth => currentHealth;

    public bool IsAlive => currentHealth > 0;

    [Header("Invincibility System")]
    [SerializeField] private float hitInvincibilityDuration = 0.3f;
    public float HitInvincibilityDuration
    {
        get => hitInvincibilityDuration;
        set => hitInvincibilityDuration = Mathf.Max(0f, value);
    }

    [SerializeField] private Dictionary<InvincibilityType, InvincibilityData> activeInvincibilities = new();
    [SerializeField] private Dictionary<InvincibilityType, Coroutine> invincibilityCoroutines = new();


    #region Initialization
    protected override void Awake()
    {
        base.Awake();
        InitializeHealth();
    }

    protected virtual void InitializeHealth()
    {
        currentHealth = maxHealth;
    }
    #endregion

    #region Health Management
    /// <summary>
    /// Entity가 피해를 받았을 때 호출되는 메서드.
    /// 매개변수로 DamageData를 받으며, 실제 게임에서는 이 구조를 사용해야 함.
    /// Legacy 메서드인 TakeDamage(int damage)는 디버그용으로만 사용
    /// </summary>  
    public virtual void TakeDamage(DamageData damageData)
    {
        if (!IsAlive) return;
        
        // 무적 상태 확인
        if (IsInvincible(InvincibilityType.HitInvincibility) ||
            IsInvincible(InvincibilityType.BuffInvincibility) ||
            IsInvincible(InvincibilityType.CutsceneInvincibility))
        {
            return;
        }
        
        int damageAmount = Mathf.RoundToInt(damageData.damage);
        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        
        Debug.Log($"{entityID} took {damageAmount} damage from {damageData.damageSource?.GetType().Name}. Health: {currentHealth}/{maxHealth}");
        
        // 1. 무적 시작
        float invincibilityDuration = CalculateHitInvincibilityDuration(damageData);
        if (invincibilityDuration > 0)
        {
            StartInvincibility(InvincibilityType.HitInvincibility, invincibilityDuration);
        }
        
        // 2. 피격 애니메이션 처리 (하위 클래스에서 구현)
        OnDamageTaken(damageData);
        
        // 3. 사망 처리
        if (currentHealth <= 0)
        {
            StartCoroutine(Die());
        }
    }

    /// <summary>
    /// TakeDamageFromContextMenu 등을 위한 디버그용.
    /// 실제 게임 구현에서는 DamageData를 사용해야 함.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void TakeDamage(int damage)
    {
        DamageData legacyDamage = new()
        {
            attacker = null,
            target = this,
            damageSource = null,
            damage = damage
        };
        TakeDamage(legacyDamage);
    }

    public virtual void Heal(int amount)
    {
        if (!IsAlive) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealed(amount);
    }
    #endregion

    #region Health Events
    protected virtual void OnDamageTaken(DamageData damageData) { }
    protected virtual void OnHealed(int amount) { }

    ///<summary>
    /// Entity사망 때 연출 및 처리 등이 필요한 경우. <!> Die() 코루틴이 실행될 때 호출되며, OnDie가 끝난 후 오브젝트 비활성화되므로, OnDie 안에서 오브젝트 비활성화하지 않아도 됨.
    ///</summary>
    protected virtual IEnumerator OnDie() { yield return null; }

    protected virtual IEnumerator Die()
    {
        yield return StartCoroutine(OnDie());
        gameObject.SetActive(false);
    }
    #endregion

    #region Invincibility Management
    public virtual void StartInvincibility(InvincibilityType type, float duration, bool showEffect = true)
    {
        if (duration <= 0) return;

        // 기존 동일 타입 무적이 있다면 정리
        if (activeInvincibilities.ContainsKey(type))
        {
            StopInvincibility(type);
        }

        // 새 무적 데이터 생성
        InvincibilityData newInvincibility = new InvincibilityData(type, duration, showEffect);
        activeInvincibilities[type] = newInvincibility;

        // 코루틴 시작
        Coroutine invincibilityCoroutine = StartCoroutine(InvincibilityCoroutine(newInvincibility));
        invincibilityCoroutines[type] = invincibilityCoroutine;
    }

    public virtual void StopInvincibility(InvincibilityType type)
    {
        if (invincibilityCoroutines.ContainsKey(type))
        {
            StopCoroutine(invincibilityCoroutines[type]);
            invincibilityCoroutines.Remove(type);
        }

        if (activeInvincibilities.ContainsKey(type))
        {
            activeInvincibilities.Remove(type);
        }
    }

    private IEnumerator InvincibilityCoroutine(InvincibilityData invincibilityData)
    {
        while (invincibilityData.remainingTime > 0)
        {
            invincibilityData.remainingTime -= Time.deltaTime;
            yield return null;
        }

        // 시간 만료로 무적 해제
        StopInvincibility(invincibilityData.type);
    }

    public virtual void ClearAllInvincibilities()
    {
        var types = activeInvincibilities.Keys.ToList();
        foreach (var type in types)
        {
            StopInvincibility(type);
        }
    }
    #endregion

    #region Invincibility Status
    public virtual bool IsInvincible(InvincibilityType specificType = InvincibilityType.None)
    {
        if (specificType != InvincibilityType.None)
        {
            return activeInvincibilities.ContainsKey(specificType);
        }

        return activeInvincibilities.Count > 0;
    }

    public virtual InvincibilityType GetHighestPriorityInvincibility()
    {
        if (activeInvincibilities.Count == 0) return InvincibilityType.None;

        return activeInvincibilities.Keys
            .OrderByDescending(type => InvincibilityHelper.GetPriority(type))
            .First();
    }
    #endregion

    #region Invincibility Utilities
    protected virtual float CalculateHitInvincibilityDuration(DamageData damageData)
    {
        // 방식 1: 고정 무적시간
        return hitInvincibilityDuration;

        // 방식 2: 피해량 비례 (주석 처리)
        // return Mathf.Clamp(damageData.damage * 0.05f, 0.1f, 1.0f);

        // 방식 3: 체력 비례 (주석 처리)
        // float healthRatio = currentHealth / (float)maxHealth;
        // return healthRatio < 0.3f ? 1.0f : 0.3f;
    }
    #endregion
}
