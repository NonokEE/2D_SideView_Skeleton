// Assets/01_Scripts/Weapon/MachineGunWeapon.cs
using UnityEngine;
using System.Collections;

public class MachineGunWeapon : BaseWeapon
{
    [Header("Machine Gun Settings")]
    public GameObject bulletPrefab;
    public BulletPhysicsConfig bulletConfig;

    [Header("Fire Rate Settings")]
    public float burstFireRate = 8f;

    private Coroutine burstFireCoroutine;
    private bool isFiring;

    private Vector2 currentAimDir = Vector2.right;

    public override void OnLeftDown(Vector2 aimDirection)
    {
        if (!CanFire()) return;

        if (aimDirection.sqrMagnitude > 0.0001f) currentAimDir = aimDirection.normalized;

        Fire(currentAimDir);
        UpdateFireTime();
        StartBurstFire();
    }

    public override void OnLeftHold(Vector2 aimDirection)
    {
        if (aimDirection.sqrMagnitude > 0.0001f) currentAimDir = aimDirection.normalized;
    }

    public override void OnLeftUp(Vector2 aimDirection)
    {
        StopBurstFire();
    }

    public override void OnUnequip()
    {
        // 전환 시 연사 즉시 중지
        StopBurstFire();
        base.OnUnequip();
    }

    private void StartBurstFire()
    {
        if (isFiring) return;
        isFiring = true;
        burstFireCoroutine = StartCoroutine(BurstFireCoroutine());
    }

    private void StopBurstFire()
    {
        if (burstFireCoroutine != null)
        {
            StopCoroutine(burstFireCoroutine);
            burstFireCoroutine = null;
        }
        isFiring = false;
    }

    private IEnumerator BurstFireCoroutine()
    {
        float interval = 1f / burstFireRate;
        yield return new WaitForSeconds(interval);

        while (isFiring)
        {
            Fire(currentAimDir);
            yield return new WaitForSeconds(interval);
        }
    }

    private void Fire(Vector2 direction)
    {
        if (bulletPrefab == null || bulletConfig == null || owner == null) return;
        if (owner is not PlayerEntity playerEntity) return;

        Vector3 firePosition = playerEntity.GetFirePosition(direction);
        GameObject bulletObj = (PoolManager.Instance != null)
            ? PoolManager.Instance.Spawn(bulletPrefab, firePosition, Quaternion.identity)
            : Instantiate(bulletPrefab, firePosition, Quaternion.identity);

        if (!bulletObj.TryGetComponent<BulletDamageSource>(out var bulletComponent)) return;

        bulletComponent.Initialize(owner, this, bulletConfig);
        bulletComponent.SetDirection(direction);
    }

    private void OnDisable() => StopBurstFire();
    private void OnDestroy() => StopBurstFire();
}
