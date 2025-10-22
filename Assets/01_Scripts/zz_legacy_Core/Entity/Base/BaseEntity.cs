using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseEntity : MonoBehaviour
{
    [Header("Entity Containers")]
    [SerializeField] protected VisualContainer visualContainer;
    [SerializeField] protected PhysicsContainer physicsContainer;
    public VisualContainer Visual => visualContainer;
    public PhysicsContainer Physics => physicsContainer;

    [Header("Entity Properties")]
    public string entityID;

    [Header("Health System")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int currentHealth;
    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsAlive => currentHealth > 0;

    [Header("Invincibility System")]
    [SerializeField] private float hitInvincibilityDuration = 0.3f; // Inspector에서 조정 가능
    public float HitInvincibilityDuration
    {
        get => hitInvincibilityDuration;
        set => hitInvincibilityDuration = Mathf.Max(0f, value); // 음수 방지
    }

    [SerializeField] private Dictionary<InvincibilityType, InvincibilityData> activeInvincibilities 
        = new Dictionary<InvincibilityType, InvincibilityData>();
    [SerializeField] private Dictionary<InvincibilityType, Coroutine> invincibilityCoroutines 
        = new Dictionary<InvincibilityType, Coroutine>();

    [Header("Physics Components (Root)")]
    protected Rigidbody2D entityRigidbody;
    public Rigidbody2D EntityRigidbody => entityRigidbody;

    // Initialization Methods
    protected virtual void Awake()
    {
        InitializeComponents();
        InitializeContainers();
        InitializeHealth();
        Initialize();
    }

    protected virtual void InitializeComponents()
    {
        entityRigidbody = GetComponent<Rigidbody2D>();

        if (entityRigidbody == null)
        {
            Debug.LogError($"{gameObject.name}: Rigidbody2D component is required on Entity root!");
        }
    }

    protected virtual void InitializeContainers()
    {
        if (visualContainer == null)
            visualContainer = GetComponentInChildren<VisualContainer>();

        if (physicsContainer == null)
            physicsContainer = GetComponentInChildren<PhysicsContainer>();

        if (visualContainer == null)
        {
            GameObject visualObj = new GameObject("VisualContainer");
            visualObj.transform.SetParent(transform);
            visualObj.transform.localPosition = Vector3.zero;
            visualContainer = visualObj.AddComponent<VisualContainer>();
        }

        if (physicsContainer == null)
        {
            GameObject physicsObj = new GameObject("PhysicsContainer");
            physicsObj.transform.SetParent(transform);
            physicsObj.transform.localPosition = Vector3.zero;
            physicsContainer = physicsObj.AddComponent<PhysicsContainer>();
        }
    }

    protected virtual void InitializeHealth()
    {
        currentHealth = maxHealth;
    }

    public abstract void Initialize();

    // Health Management Methods
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
            Die();
        }
    }

    /// <summary>
    /// TakeDamageFromContextMenu 등을 위한 디버그용.
    /// 실제 게임 구현에서는 DamageData를 사용해야 함.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void TakeDamage(int damage)
    {
        DamageData legacyDamage = new DamageData
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

    protected virtual void OnDamageTaken(DamageData damageData) { }
    protected virtual void OnHealed(int amount) { }
    protected virtual void OnDie() { }

    protected virtual void Die()
    {
        Debug.Log($"{entityID} died!");
        OnDie();
    }
    
    // Invincibility Management Methods
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
    
    // 무적 중지
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
    
    // 무적 상태 확인
    public virtual bool IsInvincible(InvincibilityType specificType = InvincibilityType.None)
    {
        if (specificType != InvincibilityType.None)
        {
            return activeInvincibilities.ContainsKey(specificType);
        }
        
        return activeInvincibilities.Count > 0;
    }
    
    // 가장 높은 우선순위 무적 타입 반환
    public virtual InvincibilityType GetHighestPriorityInvincibility()
    {
        if (activeInvincibilities.Count == 0) return InvincibilityType.None;
        
        return activeInvincibilities.Keys
            .OrderByDescending(type => InvincibilityHelper.GetPriority(type))
            .First();
    }
    
    // 무적 코루틴
    private System.Collections.IEnumerator InvincibilityCoroutine(InvincibilityData invincibilityData)
    {
        while (invincibilityData.remainingTime > 0)
        {
            invincibilityData.remainingTime -= Time.deltaTime;
            yield return null;
        }
        
        // 시간 만료로 무적 해제
        StopInvincibility(invincibilityData.type);
    }
    
    // 모든 무적 해제
    public virtual void ClearAllInvincibilities()
    {
        var types = activeInvincibilities.Keys.ToList();
        foreach (var type in types)
        {
            StopInvincibility(type);
        }
    }

    // 피격 무적시간 계산 (가상 메서드로 하위 클래스에서 오버라이드 가능)
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

    // Physics Utility Methods
    public Vector2 GetVelocity()
    {
        return entityRigidbody?.linearVelocity ?? Vector2.zero;
    }

    public void SetVelocity(Vector2 velocity)
    {
        if (entityRigidbody != null)
            entityRigidbody.linearVelocity = velocity;
    }

    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force)
    {
        if (entityRigidbody != null)
            entityRigidbody.AddForce(force, mode);
    }

    // Visual Utility Methods
    public void SetSprite(Sprite sprite)
    {
        visualContainer?.SetSprite(sprite);
    }

    public void SetColor(Color color)
    {
        visualContainer?.SetColor(color);
    }

    public void ResetVisualToOriginal()
    {
        visualContainer?.ResetToOriginalValues();
    }

    // Collision Detection Methods
    public bool IsGrounded() => physicsContainer?.IsGrounded() ?? false;

    public bool IsOnPlatform() => physicsContainer?.IsOnPlatform() ?? false;

    public bool IsWallLeft() => physicsContainer?.IsWallLeft() ?? false;

    public bool IsWallRight() => physicsContainer?.IsWallRight() ?? false;

    public bool IsWall(float direction) => physicsContainer?.IsWall(direction) ?? false;

    public bool IsCeiling() => physicsContainer?.IsCeiling() ?? false;
}
