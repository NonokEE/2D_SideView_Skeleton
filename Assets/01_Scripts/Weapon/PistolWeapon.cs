using UnityEngine;

public class PistolWeapon : BaseWeapon
{
    [Header("Pistol Settings")]
    public DamageSourceEntity bulletPrefab;
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
        Debug.Log("Fire method called!");
        
        if (bulletPrefab == null)
        {
            Debug.LogError("Bullet Prefab is null!");
            return;
        }
        
        if (owner == null)
        {
            Debug.LogError("Owner is null!");
            return;
        }
        
        PlayerEntity playerEntity = owner as PlayerEntity;
        if (playerEntity == null)
        {
            Debug.LogError("Owner is not PlayerEntity!");
            return;
        }
        
        Transform firePoint = playerEntity.GetFirePoint();
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is null!");
            return;
        }
        
        Debug.Log($"Creating bullet at {firePoint.position}");
        
        // 탄환 생성
        DamageSourceEntity bullet = Instantiate(bulletPrefab, firePoint.position, 
            Quaternion.LookRotation(Vector3.forward, direction));
        
        Debug.Log("Bullet created successfully!");
        
        // DamageSource 초기화
        StraightBullet bulletComponent = bullet.GetComponent<StraightBullet>();
        if (bulletComponent != null)
        {
            bulletComponent.Initialize(owner, this);
            Debug.Log("Bullet initialized!");
        }
        else
        {
            Debug.LogError("StraightBullet component not found!");
        }
    }

}
