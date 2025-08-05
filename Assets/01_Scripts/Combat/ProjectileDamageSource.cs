using System.Collections;
using UnityEngine;

public abstract class ProjectileDamageSource : DamageSourceEntity
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public bool destroyOnHit = true;
    public float lifeTime = 5f;
    
    protected Vector2 moveDirection = Vector2.right;
    protected Coroutine lifetimeCoroutine;
    
    protected override void Awake()
    {
        base.Awake();

        if (lifeTime > 0)
            lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    
    public virtual void SetDirection(Vector2 direction)
    {
        moveDirection = direction.normalized;

        // 탄환 회전 설정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 즉시 velocity 업데이트
        if (entityRigidbody != null)
        {
            entityRigidbody.linearVelocity = moveDirection * speed;
        }

        //*Debug.Log($"Projectile direction set to: {moveDirection}, velocity: {entityRigidbody?.linearVelocity}");
    }
    
    private IEnumerator LifetimeCoroutine()
    {
        yield return new WaitForSeconds(lifeTime);
        DestroyProjectile();
    }
    
    protected virtual void InitializeMovement()
    {
        if (entityRigidbody != null && entityRigidbody.linearVelocity.magnitude < 0.1f)
        {
            entityRigidbody.linearVelocity = moveDirection * speed;
        }
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
