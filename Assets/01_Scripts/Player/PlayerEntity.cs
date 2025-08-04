using UnityEngine;

public class PlayerEntity : LivingEntity
{
    [Header("Player Specific")]
    private PlayerAnimationManager animationManager;

    [Header("Weapon System")]
    [SerializeField] private BaseWeapon currentWeapon;
    [SerializeField] private Transform firePoint;
    
    protected override void Awake()
    {
        base.Awake();
        animationManager = GetComponent<PlayerAnimationManager>();
    }
    
    public override void Initialize()
    {
        Debug.Log($"Player {entityID} initialized");
        
        // 기본 설정
        if (string.IsNullOrEmpty(entityID))
            entityID = "Player_01";
            
        // Rigidbody2D 기본 설정
        if (entityRigidbody != null)
        {
            entityRigidbody.bodyType = RigidbodyType2D.Dynamic;
            entityRigidbody.gravityScale = 1f;
            entityRigidbody.freezeRotation = true;
        }
    }
    
    public override void Jump()
    {
        if (!IsGrounded()) return;
        
        // 점프 애니메이션 실행
        if (animationManager != null)
            animationManager.PlayJumpAnimation();
            
        base.Jump();
    }
    
    public override void TakeDamage(DamageData damageData)
    {
        base.TakeDamage(damageData);
        // 피격 애니메이션 실행
        if (animationManager != null)
            animationManager.PlayHitAnimation();
            
        Debug.Log($"Player took {damageData.damage} damage");
    }
    
    // Weapon 관련 메서드
    public void SetWeapon(BaseWeapon weapon)
    {
        currentWeapon = weapon;
        if (currentWeapon != null)
            currentWeapon.Initialize(this);
    }

    public void HandleWeaponInput(MouseInputType MouseinputType, Vector2 aimDirection)
    {
        if (currentWeapon == null) return;
        
        switch (MouseinputType)
        {
            case MouseInputType.LeftDown:
                currentWeapon.OnLeftDown(aimDirection);
                break;
            case MouseInputType.LeftHold:
                currentWeapon.OnLeftHold(aimDirection);
                break;
            case MouseInputType.LeftUp:
                currentWeapon.OnLeftUp(aimDirection);
                break;
            case MouseInputType.RightDown:
                currentWeapon.OnRightDown(aimDirection);
                break;
            case MouseInputType.RightHold:
                currentWeapon.OnRightHold(aimDirection);
                break;
            case MouseInputType.RightUp:
                currentWeapon.OnRightUp(aimDirection);
                break;
        }
    }

    public Transform GetFirePoint()
    {
        return firePoint;
    }
    
    // 디버깅용 메서드
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // Inspector에서 설정 확인
        if (entityRigidbody == null)
            entityRigidbody = GetComponent<Rigidbody2D>();
    }
}
