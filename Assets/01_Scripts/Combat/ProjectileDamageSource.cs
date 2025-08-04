using System.Collections;
using UnityEngine;

public abstract class ProjectileDamageSource : DamageSourceEntity
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public bool destroyOnHit = true;
    public float lifeTime = 5f;
    
    protected Coroutine lifetimeCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeMovement();
        
        // Coroutine으로 생명주기 관리
        if (lifeTime > 0)
            lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }
    
    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        DestroyProjectile();
    }
    
    protected virtual void InitializeMovement()
    {
        // 기본 직선 이동 (transform.right 방향)
        if (entityRigidbody != null)
            entityRigidbody.linearVelocity = transform.right * speed;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ground 레이어와 충돌 시 파괴
        if (IsGroundLayer(other.gameObject.layer))
        {
            OnHitGround();
            return;
        }
        
        // Entity와 충돌 처리
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target != null && CanDamage(target))
        {
            DamageData damageData = GenerateDamageData(target);
            target.TakeDamage(damageData); // 매개변수를 damageData로 통일
            
            OnHitTarget(target);
            
            if (destroyOnHit)
                DestroyProjectile();
        }
    }
    
    private bool IsGroundLayer(int layer)
    {
        return layer == 6; // Ground 레이어
    }
    
    protected virtual void OnHitTarget(BaseEntity target) 
    {
        Debug.Log($"Projectile hit {target.entityID} for {damage} damage");
    }
    
    protected virtual void OnHitGround() 
    {
        Debug.Log("Projectile hit ground");
        DestroyProjectile();
    }
    
    protected virtual void DestroyProjectile()
    {
        // Coroutine 정리
        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }
        
        gameObject.SetActive(false);
    }
}
