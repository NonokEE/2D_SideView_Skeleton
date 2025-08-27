using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldDamageSource : DamageSourceEntity
{
    [Header("Field Settings")]
    [SerializeField] private float fieldDuration = 5f;
    [SerializeField] private float damageInterval = 0.5f;
    [SerializeField] private float fieldRadius = 1f;

    private readonly HashSet<BaseEntity> entitiesInField = new();
    private Coroutine fieldCoroutine;

    public override void Initialize()
    {
        SetupFieldAppearance();
        fieldCoroutine = StartCoroutine(FieldDamageCoroutine());
    }

    public override void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        base.Initialize(sourceOwner, sourceWeapon);

        if (physicsContainer?.MainCollider is CircleCollider2D circle)
        {
            circle.isTrigger = true;
            circle.radius = fieldRadius;
        }

        Initialize();
    }

    private void SetupFieldAppearance()
    {
        if (visualContainer == null) return;

        var c = Color.green; c.a = 0.6f;
        visualContainer.SetColor(c);

        visualContainer.transform.localScale = Vector3.one * (fieldRadius * 2f);
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

        DestroySelf();
    }

    private void ApplyFieldDamage()
    {
        foreach (var e in entitiesInField)
        {
            if (e == null || !e.IsAlive) continue;
            if (!CanDamage(e)) continue;

            var data = GenerateDamageData(e);
            e.TakeDamage(data);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var e = other.GetComponent<BaseEntity>();
        if (e != null) entitiesInField.Add(e);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var e = other.GetComponent<BaseEntity>();
        if (e != null) entitiesInField.Remove(e);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, fieldRadius);
    }

    // ─── Pool Hooks ─────────────────────────────────────────────────────
    public override void OnDespawned()
    {
        if (fieldCoroutine != null)
        {
            StopCoroutine(fieldCoroutine);
            fieldCoroutine = null;
        }
    }

    public override void ResetForPool()
    {
        base.ResetForPool();
        entitiesInField.Clear();

        if (fieldCoroutine != null)
        {
            StopCoroutine(fieldCoroutine);
            fieldCoroutine = null;
        }
    }
}
