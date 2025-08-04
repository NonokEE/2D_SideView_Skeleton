// Assets/01_Scripts/Entity/ProjectileEntity.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileEntity : BaseDamageSource
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public bool destroyOnHit = true;
    public bool canPierce = false; // 관통 여부
    
    private Vector2 direction;
    private Rigidbody2D rb;
    private bool hasHit = false;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 0f;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }
    
    public void Initialize(BaseEntity attacker, BaseWeapon weapon, Vector2 direction, float damage)
    {
        this.direction = direction.normalized;
        base.Initialize(attacker, weapon);
        
        rb.linearVelocity = this.direction * speed;
        
        // 수명 제한
        Destroy(gameObject, lifeTime);
    }
    
    protected override void SetDefaultStrategies()
    {
        enterStrategy = new InstantDamageStrategy();
        stayStrategy = null;
        exitStrategy = null;
    }

    // 관통 처리를 위한 ExecuteEnter 오버라이드
    public override void ExecuteEnter(BaseEntity target)
    {
        if (!IsTargetInteractable(target)) return;
        if (hasHit && !canPierce) return; // 이미 맞았고 관통 불가면 무시

        currentTargets.Add(target);
        enterStrategy?.OnAttackEnter(target, this);

        if (!canPierce) hasHit = true; // 관통 불가능하면 맞춤 표시
    }

    public override void ExecuteStay(BaseEntity target)
    {
        // 탄환은 Stay 이벤트 사용 안함 (필요시 오버라이드)
    }

    public override void ExecuteExit(BaseEntity target)
    {
        if (!IsTargetInteractable(target)) return;
        currentTargets.Remove(target);
        exitStrategy?.OnAttackExit(target, this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target == null) return;

        ExecuteEnter(target);

        if (!canPierce && destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target == null) return;

        ExecuteStay(target);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target == null) return;

        ExecuteExit(target);
    }

    public override DamageData GenerateDamageData(BaseEntity target)
    {
        float damageValue = weapon != null ? weapon.damage : 10f;
        return new DamageData(attacker, weapon, target, damageValue, direction);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
