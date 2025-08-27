using UnityEngine;

public class PistolWeapon : BaseWeapon
{
    [Header("Pistol Settings")]
    public GameObject bulletPrefab;
    public BulletPhysicsConfig bulletConfig;

    public override void OnLeftDown(Vector2 aimDirection)
    {
        if (!CanFire()) return;
        Fire(aimDirection);
        UpdateFireTime();
    }

    private void Fire(Vector2 direction)
    {
        if (bulletPrefab == null || bulletConfig == null || owner == null)
        {
            Debug.LogWarning("PistolWeapon: Missing bulletPrefab or bulletConfig or owner!");
            return;
        }

        if (owner is not PlayerEntity playerEntity)
        {
            Debug.LogWarning("PistolWeapon: owner is not PlayerEntity!");
            return;
        }

        Vector3 firePosition = playerEntity.GetFirePosition(direction);

        GameObject bulletObj = (PoolManager.Instance != null)
            ? PoolManager.Instance.Spawn(bulletPrefab, firePosition, Quaternion.identity)
            : Instantiate(bulletPrefab, firePosition, Quaternion.identity);

        if (!bulletObj.TryGetComponent<BulletDamageSource>(out var bulletComponent))
        {
            Debug.LogError("PistolWeapon: BulletDamageSource not found on prefab!");
            return;
        }

        bulletComponent.Initialize(owner, this, bulletConfig);
        bulletComponent.SetDirection(direction);
    }
}
