using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Properties")]
    public string weaponName;
    public int damage = 10;
    public float fireRate = 1f;

    protected float lastFireTime;
    protected BaseEntity owner;

    public virtual void Initialize(BaseEntity weaponOwner)
    {
        owner = weaponOwner;
    }

    // 수명주기 훅(장착/비장착)
    public virtual void OnEquip(BaseEntity weaponOwner)
    {
        // 기본 구현: 소유자 세팅 + 필요시 상태 초기화
        Initialize(weaponOwner);
    }

    public virtual void OnUnequip()
    {
        // 기본 구현: 필요한 경우 하위 무기에서 코루틴/상태 정지
    }

    // 입력 전략
    public virtual void OnLeftDown(Vector2 aimDirection) { }
    public virtual void OnLeftHold(Vector2 aimDirection) { }
    public virtual void OnLeftUp(Vector2 aimDirection) { }
    public virtual void OnRightDown(Vector2 aimDirection) { }
    public virtual void OnRightHold(Vector2 aimDirection) { }
    public virtual void OnRightUp(Vector2 aimDirection) { }

    protected bool CanFire()
    {
        return Time.time >= lastFireTime + (1f / fireRate);
    }

    protected void UpdateFireTime()
    {
        lastFireTime = Time.time;
    }
}
