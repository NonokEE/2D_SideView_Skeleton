// Assets/01_Scripts/Combat/ExplosionDamageSource.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionDamageSource : DamageSourceEntity
{
    [Header("Explosion Settings")]
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private float explosionDuration = 0.3f;
    
    private bool hasExploded = false;
    private HashSet<BaseEntity> damagedEntities = new HashSet<BaseEntity>();
    
    public override void Initialize()
    {
        if (!hasExploded)
        {
            StartCoroutine(ExplodeCoroutine());
        }
    }
    
    public override void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        base.Initialize(sourceOwner, sourceWeapon);
        
        // 폭발 범위 설정
        if (physicsContainer?.MainCollider is CircleCollider2D circleCollider)
        {
            circleCollider.radius = explosionRadius;
        }
        
        Initialize();
    }
    
    private IEnumerator ExplodeCoroutine()
    {
        yield return null;
        hasExploded = true;
        
        // ✅ VisualContainer에게 애니메이션 위임
        if (visualContainer != null)
        {
            // ScaleStretch 애니메이션 (인덱스 0)
            visualContainer.PlayGeometricAnimation(0);
            
            // ColorBlink 애니메이션 (인덱스 1) - 독립적으로 실행
            visualContainer.PlayGeometricAnimation(1);
        }
        
        // 폭발 지속시간 대기
        yield return new WaitForSeconds(explosionDuration);
        
        // 폭발 정리
        gameObject.SetActive(false);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!hasExploded) return;
        
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target != null && CanDamage(target) && !damagedEntities.Contains(target))
        {
            damagedEntities.Add(target);
            
            DamageData damageData = GenerateDamageData(target);
            target.TakeDamage(damageData);
            
            Debug.Log($"Explosion hit {target.entityID} for {damage} damage");
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
