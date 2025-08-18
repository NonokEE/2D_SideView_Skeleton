using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldDamageSource : DamageSourceEntity
{
    [Header("Field Settings")]
    [SerializeField] private float fieldDuration = 5f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private float fieldRadius = 1f;
    
    private HashSet<BaseEntity> entitiesInField = new HashSet<BaseEntity>();
    private Coroutine fieldCoroutine;
    
    public override void Initialize()
    {
        SetupFieldAppearance();
        fieldCoroutine = StartCoroutine(FieldDamageCoroutine());
    }
    
    public override void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        base.Initialize(sourceOwner, sourceWeapon);
        
        // 필드 크기 설정
        if (physicsContainer?.MainCollider is CircleCollider2D circleCollider)
        {
            circleCollider.radius = fieldRadius;
        }
        
        Initialize();
    }
    
    private void SetupFieldAppearance()
    {
        if (visualContainer != null)
        {
            Color fieldColor = Color.green;
            fieldColor.a = 0.6f;
            visualContainer.SetColor(fieldColor);
            
            // 필드 크기 설정
            visualContainer.transform.localScale = Vector3.one * (fieldRadius * 2);
        }
    }
    
    private IEnumerator FieldDamageCoroutine()
    {
        float elapsed = 0f;
        
        while (elapsed < fieldDuration)
        {
            ApplyFieldDamage();
            
            yield return new WaitForSeconds(damageInterval);
            elapsed += damageInterval;
        }
        
        // 필드 소멸
        gameObject.SetActive(false);
    }
    
    private void ApplyFieldDamage()
    {
        foreach (var entity in entitiesInField)
        {
            if (entity == null || !entity.IsAlive) continue;
            
            if (CanDamage(entity))
            {
                DamageData damageData = GenerateDamageData(entity);
                entity.TakeDamage(damageData);
                Debug.Log($"Field damaged {entity.entityID} for {damage} damage");
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        BaseEntity entity = other.GetComponent<BaseEntity>();
        if (entity != null)
        {
            entitiesInField.Add(entity);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        BaseEntity entity = other.GetComponent<BaseEntity>();
        if (entity != null)
        {
            entitiesInField.Remove(entity);
        }
    }
    
    private void OnDestroy()
    {
        if (fieldCoroutine != null)
        {
            StopCoroutine(fieldCoroutine);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fieldRadius);
    }
}
