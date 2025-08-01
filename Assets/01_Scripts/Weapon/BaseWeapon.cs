using UnityEngine;

public abstract class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Properties")]
    public string weaponName;
    public float damage = 10f;
    public float fireRate = 1f;
    
    protected float lastFireTime;
    protected BaseEntity owner;
    
    public virtual void Initialize(BaseEntity weaponOwner)
    {
        owner = weaponOwner;
    }
    
    // 입력별 전략 메서드들 (하위 클래스에서 오버라이드)
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
