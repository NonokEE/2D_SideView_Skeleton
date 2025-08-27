using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionDamageSource : DamageSourceEntity
{
    #region Inspector Fields
    [Header("Explosion Settings")]
    [SerializeField] private Vector2 explosionSize = new(3f, 3f);
    [SerializeField] private float explosionDuration = 0.3f;

    [Header("Debug Visualization")]
    [SerializeField] private bool showExplosionGizmos = true;
    #endregion

    #region Private Fields
    private bool hasExploded;
    private readonly HashSet<BaseEntity> damagedEntities = new();
    private Coroutine explodeCoroutine;
    #endregion

    #region Initialization
    public override void Initialize()
    {
        if (!hasExploded)
            explodeCoroutine = StartCoroutine(ExplodeCoroutine());
    }

    public override void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        base.Initialize(sourceOwner, sourceWeapon);
        SetupExplosionCollider();
        Initialize();
    }

    private void SetupExplosionCollider()
    {
        if (physicsContainer?.MainCollider is CapsuleCollider2D capsule)
        {
            capsule.size = explosionSize;
            capsule.direction = CapsuleDirection2D.Vertical;
        }
        else
        {
            Debug.LogWarning($"{name}: MainCollider is not CapsuleCollider2D!");
        }
    }
    #endregion

    #region Explosion Logic
    private IEnumerator ExplodeCoroutine()
    {
        yield return null; // 컴포넌트 Awake 보장
        hasExploded = true;
        PlayExplosionAnimation();

        yield return new WaitForSeconds(explosionDuration);
        DestroySelf();
    }

    private void PlayExplosionAnimation()
    {
        if (visualContainer == null) return;
        visualContainer.PlayGeometricAnimation(0); // ScaleStretch
        visualContainer.PlayGeometricAnimation(1); // ColorBlink
    }
    #endregion

    #region Damage Handling
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!hasExploded) return;

        var target = other.GetComponent<BaseEntity>();
        if (target != null && CanDamage(target) && !damagedEntities.Contains(target))
        {
            damagedEntities.Add(target);
            var data = GenerateDamageData(target);
            target.TakeDamage(data);
        }
    }
    #endregion

    #region Debug Visualization
    private void OnDrawGizmos()
    {
        if (!showExplosionGizmos) return;
        var pos = transform.position;

        Gizmos.color = (Application.isPlaying && hasExploded) ? new Color(1f, 0f, 0f, 0.7f) : new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireCube(pos, explosionSize);

        if (Application.isPlaying && hasExploded)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
            Gizmos.DrawCube(pos, explosionSize);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showExplosionGizmos) return;
        var pos = transform.position;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos, explosionSize);
        Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        Gizmos.DrawCube(pos, explosionSize);
    }
    #endregion

    #region Pool Hooks
    public override void OnSpawned()
    {
        hasExploded = false;
        damagedEntities.Clear();
    }

    public override void OnDespawned()
    {
        if (explodeCoroutine != null)
        {
            StopCoroutine(explodeCoroutine);
            explodeCoroutine = null;
        }
    }

    public override void ResetForPool()
    {
        base.ResetForPool();
        hasExploded = false;
        damagedEntities.Clear();
        if (explodeCoroutine != null)
        {
            StopCoroutine(explodeCoroutine);
            explodeCoroutine = null;
        }
    }
    #endregion
}
