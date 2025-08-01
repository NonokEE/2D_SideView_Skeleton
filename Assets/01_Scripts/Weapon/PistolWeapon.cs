using UnityEngine;

public class PistolWeapon : BaseWeapon
{
    [Header("Pistol Settings")]
    public GameObject bulletPrefab; // 임시로 null, 다음 단계에서 구현

    public override void OnLeftDown(Vector2 aimDirection)
    {
        if (CanFire())
        {
            Fire(aimDirection);
            UpdateFireTime();
        }
    }

    private void Fire(Vector2 direction)
    {
        Debug.Log($"Pistol fired! Direction: {direction}");

        // TODO: 다음 단계에서 실제 총알 생성
        // if (bulletPrefab != null && owner != null)
        // {
        //     Transform firePoint = (owner as PlayerEntity)?.GetFirePoint();
        //     if (firePoint != null)
        //     {
        //         Instantiate(bulletPrefab, firePoint.position, Quaternion.LookRotation(Vector3.forward, direction));
        //     }
        // }
    }
}
