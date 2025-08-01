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

        if (bulletPrefab != null && owner != null)
        {
            PlayerEntity playerEntity = owner as PlayerEntity;
            Transform firePoint = playerEntity?.GetFirePoint();

            if (firePoint != null)
            {
                // 총알 생성
                GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
                ProjectileEntity projectile = bulletObj.GetComponent<ProjectileEntity>();

                if (projectile != null)
                {
                    projectile.Initialize(owner, this, direction, damage);
                }
            }
        }
    }
}