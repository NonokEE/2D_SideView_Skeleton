using UnityEngine;

public class PlayerEntity : LivingEntity
{
    [Header("Player Specific")]
    private PlayerAnimationHandler animationHandler;

    [Header("Weapon System (Legacy)")]
    [SerializeField] private GameObject weaponPrefab;       // 기존 단일 무기 방식(Manager 사용 시 미사용)
    [SerializeField] private float fireDistance = 0.8f;

    [Space(10)]
    [SerializeField] private BaseWeapon currentWeapon;      // 현재 장착 무기 참조

    [Header("Weapon Manager (New)")]
    [SerializeField] private WeaponManager weaponManager;   // 인스펙터에 추가(없으면 GetComponent로 확인)
    [SerializeField] private Transform weaponSocket;        // Player 루트 하위의 "WeaponSocket" 지정 권장

    public WeaponManager WeaponManager => weaponManager;

    protected override void Awake()
    {
        base.Awake();
        if (animationHandler == null) animationHandler = GetComponent<PlayerAnimationHandler>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
    }

    public override void Initialize()
    {
        Debug.Log($"Player {entityID} initialized");

        if (string.IsNullOrEmpty(entityID)) entityID = "Player_01";
        currentHealth = maxHealth;

        // 신규 무기 시스템 우선
        if (weaponManager != null)
        {
            if (weaponSocket != null)
                weaponManager.SetWeaponSocket(weaponSocket);

            weaponManager.Initialize(this);
            currentWeapon = weaponManager.Current;
            weaponManager.OnWeaponChanged += w => currentWeapon = w;
        }
        else
        {
            // 레거시 단일 무기 초기화
            InitializeWeaponLegacy();
        }

        if (entityRigidbody != null)
        {
            entityRigidbody.bodyType = RigidbodyType2D.Dynamic;
            entityRigidbody.gravityScale = 1f;
            entityRigidbody.freezeRotation = true;
        }
    }

    private void InitializeWeaponLegacy()
    {
        if (weaponPrefab != null)
        {
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

    public override void Jump()
    {
        if (!IsGrounded()) return;
        if (animationHandler != null) animationHandler.PlayJumpAnimation();
        base.Jump();
    }

    protected override void OnDamageTaken(DamageData damageData)
    {
        base.OnDamageTaken(damageData);
        if (animationHandler != null) animationHandler.PlayHitAnimation();
    }

    // 입력 위임
    public void HandleWeaponInput(MouseInputType mouseInputType, Vector2 aimDirection)
    {
        if (currentWeapon == null)
        {
            Debug.LogWarning("No weapon available for input");
            return;
        }

        switch (mouseInputType)
        {
            case MouseInputType.LeftDown:  currentWeapon.OnLeftDown(aimDirection);  break;
            case MouseInputType.LeftHold:  currentWeapon.OnLeftHold(aimDirection);  break;
            case MouseInputType.LeftUp:    currentWeapon.OnLeftUp(aimDirection);    break;
            case MouseInputType.RightDown: currentWeapon.OnRightDown(aimDirection); break;
            case MouseInputType.RightHold: currentWeapon.OnRightHold(aimDirection); break;
            case MouseInputType.RightUp:   currentWeapon.OnRightUp(aimDirection);   break;
        }
    }

    public Vector3 GetFirePosition(Vector2 aimDirection) => transform.position + (Vector3)(aimDirection * fireDistance);
    public float GetFireDistance() => fireDistance;

    // 레거시 교체(Manager 없을 때만 사용)
    public void ChangeWeapon(GameObject newWeaponPrefab)
    {
        if (weaponManager != null)
        {
            Debug.LogWarning("WeaponManager 사용 중에는 ChangeWeapon(레거시)을 사용하지 않습니다.");
            return;
        }

        if (currentWeapon != null) Destroy(currentWeapon.gameObject);
        weaponPrefab = newWeaponPrefab;
        InitializeWeaponLegacy();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (entityRigidbody == null) entityRigidbody = GetComponent<Rigidbody2D>();
        if (weaponManager == null) weaponManager = GetComponent<WeaponManager>();
    }
#endif

    private void OnDrawGizmosSelected()
    {
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
