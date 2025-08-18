using UnityEngine;

public class PistolWeapon : BaseWeapon
{
    [Header("Pistol Settings")]
    public GameObject bulletPrefab; // 새로운 Bullet 프리팹
    public BulletPhysicsConfig bulletConfig; // ✅ Config 직접 참조
    
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
        if (bulletPrefab != null && bulletConfig != null && owner != null)
        {
            PlayerEntity playerEntity = owner as PlayerEntity;
            if (playerEntity != null)
            {
                // 동적 발사 위치 계산
                Vector3 firePosition = playerEntity.GetFirePosition(direction);
                
                // 탄환 생성 (임시: Instantiate 사용)
                GameObject bulletObj = Instantiate(bulletPrefab, firePosition, Quaternion.identity);
                
                // 새로운 BulletDamageSource 클래스 초기화
                BulletDamageSource bulletComponent = bulletObj.GetComponent<BulletDamageSource>();
                if (bulletComponent != null)
                {
                    // ✅ BulletPhysicsConfig와 함께 초기화
                    bulletComponent.Initialize(owner, this, bulletConfig);
                    bulletComponent.SetDirection(direction);
                }
                else
                {
                    Debug.LogError("PistolWeapon: Bullet component not found on prefab!");
                }
            }
        }
        else
        {
            Debug.LogWarning("PistolWeapon: Missing bulletPrefab or bulletConfig!");
        }
    }
}
