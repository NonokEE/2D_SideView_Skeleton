using UnityEngine;

public class PlayerEntity : LivingEntity
{
    [Header("Player Specific")]
    private PlayerAnimationManager animationManager;

    [Header("Weapon System")]
    [SerializeField] private GameObject weaponPrefab;
    [SerializeField] private float fireDistance = 0.8f;

    [Space(10)]
    [SerializeField] private BaseWeapon currentWeapon;

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

        currentHealth = maxHealth;

        // 무기 생성 및 초기화
        InitializeWeapon();

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

    private void InitializeWeapon()
    {
        if (weaponPrefab != null)
        {
            // 무기를 Player의 자식으로 생성
            GameObject weaponInstance = Instantiate(weaponPrefab, transform);
            currentWeapon = weaponInstance.GetComponent<BaseWeapon>();

            if (currentWeapon != null)
            {
                currentWeapon.Initialize(this);
                Debug.Log($"Weapon {currentWeapon.weaponName} initialized");
            }
            else
            {
                Debug.LogError("Weapon prefab doesn't have BaseWeapon component!");
            }
        }
        else
        {
            Debug.LogWarning("No weapon prefab assigned to PlayerEntity");
        }
    }

    public void HandleWeaponInput(MouseInputType mouseInputType, Vector2 aimDirection)
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("No weapon available for input");
            return;
        }

        switch (mouseInputType)
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

    public Vector3 GetFirePosition(Vector2 aimDirection) => transform.position + (Vector3)(aimDirection * fireDistance);
    public float GetFireDistance() => fireDistance;

    public void ChangeWeapon(GameObject newWeaponPrefab)
    {
        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject);

        weaponPrefab = newWeaponPrefab;
        InitializeWeapon();
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
    
    private void OnDrawGizmosSelected()
    {
        // 마우스 방향으로 발사 위치 표시
        if (Camera.main != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            Vector2 aimDirection = (mouseWorldPos - transform.position).normalized;
            
            Vector3 firePos = GetFirePosition(aimDirection);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePos, 0.1f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, firePos);
        }
    }
}
