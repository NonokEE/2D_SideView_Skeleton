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

    public override void OnLeftDown(Vector2 aimDirection)
    {
        if (!CanFire()) return;

        Fire(aimDirection);
        UpdateFireTime();
        StartBurstFire();
    }

    public override void OnLeftHold(Vector2 aimDirection)
    {
        // 연사 중 방향은 프레임마다 갱신
    }

    public override void OnLeftUp(Vector2 aimDirection)
    {
        StopBurstFire();
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
            Vector2 currentAim = GetCurrentAimDirection();
            Fire(currentAim);
            yield return new WaitForSeconds(interval);
        }
    }

    private Vector2 GetCurrentAimDirection()
    {
        if (Camera.main != null && owner != null)
        {
            var mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            return ((Vector2)(mouseWorld - owner.transform.position)).normalized;
        }
        return Vector2.right;
    }

    private void Fire(Vector2 direction)
    {
        if (bulletPrefab == null || bulletConfig == null || owner == null)
            return;

        if (owner is not PlayerEntity playerEntity)
            return;

        Vector3 firePosition = playerEntity.GetFirePosition(direction);

        GameObject bulletObj = (PoolManager.Instance != null)
            ? PoolManager.Instance.Spawn(bulletPrefab, firePosition, Quaternion.identity)
            : Instantiate(bulletPrefab, firePosition, Quaternion.identity);

        if (!bulletObj.TryGetComponent<BulletDamageSource>(out var bulletComponent))
            return;

        bulletComponent.Initialize(owner, this, bulletConfig);
        bulletComponent.SetDirection(direction);
    }

    private void OnDisable() => StopBurstFire();
    private void OnDestroy() => StopBurstFire();
}
