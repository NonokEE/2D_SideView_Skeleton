using UnityEngine;

public class PistolWeapon : BaseWeapon
{
    [Header("Pistol Settings")]
    public GameObject bulletPrefab;
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
        if (bulletPrefab != null && owner != null)
        {
            PlayerEntity playerEntity = owner as PlayerEntity;
            if (playerEntity != null)
            {
                // 동적 발사 위치 계산
                Vector3 firePosition = playerEntity.GetFirePosition(direction);
                
                // 탄환 생성
                GameObject bullet = Instantiate(bulletPrefab, firePosition, Quaternion.identity);
                
                // 탄환 초기화 및 방향 설정
                StraightBullet bulletComponent = bullet.GetComponent<StraightBullet>();
                if (bulletComponent != null)
                {
                    bulletComponent.Initialize(owner, this);
                    bulletComponent.SetDirection(direction); // 정확한 방향 설정
                }
                
                Debug.Log($"Bullet fired from {firePosition} in direction {direction}");
            }
        }
    }
}
