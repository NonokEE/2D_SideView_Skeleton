using UnityEngine;
using System.Collections;

public class MachineGunWeapon : BaseWeapon
{
    [Header("Machine Gun Settings")]
    public GameObject bulletPrefab; // 새로운 Bullet 프리팹
    public BulletPhysicsConfig bulletConfig; // ✅ Config 직접 참조
    
    [Header("Fire Rate Settings")]
    public float burstFireRate = 8f; // 초당 8발 연사
    
    private Coroutine burstFireCoroutine;
    private bool isFiring = false;
    
    public override void OnLeftDown(Vector2 aimDirection)
    {
        if (CanFire())
        {
            // 첫 발 즉시 발사
            Fire(aimDirection);
            UpdateFireTime();
            
            // 연속 발사 시작
            StartBurstFire(aimDirection);
        }
    }
    
    public override void OnLeftHold(Vector2 aimDirection)
    {
        // 이미 연속 발사 중 (방향은 실시간 업데이트됨)
    }
    
    public override void OnLeftUp(Vector2 aimDirection)
    {
        // 연속 발사 중지
        StopBurstFire();
    }
    
    private void StartBurstFire(Vector2 initialDirection)
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
        float burstInterval = 1f / burstFireRate;
        
        // 첫 발은 이미 발사했으므로 간격 후부터 시작
        yield return new WaitForSeconds(burstInterval);
        
        while (isFiring)
        {
            // 현재 마우스 방향 계산
            Vector2 currentAimDirection = GetCurrentAimDirection();
            
            Fire(currentAimDirection);
            
            yield return new WaitForSeconds(burstInterval);
        }
    }
    
    private Vector2 GetCurrentAimDirection()
    {
        // 현재 마우스 위치 기반 조준 방향 계산
        if (Camera.main != null && owner != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            Vector2 aimDirection = (mouseWorldPos - owner.transform.position).normalized;
            return aimDirection;
        }
        
        return Vector2.right; // 기본값
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
            }
        }
    }
    
    // 컴포넌트 정리
    private void OnDisable()
    {
        StopBurstFire();
    }
    
    private void OnDestroy()
    {
        StopBurstFire();
    }
}
