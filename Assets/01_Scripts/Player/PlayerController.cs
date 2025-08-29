using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode downKey = KeyCode.S;

    private LivingEntity controlledEntity;
    private PlayerEntity playerEntity;
    private bool isInitialized = false;
    private bool wheelConsumedThisFrame = false;

    private void Start()
    {
        controlledEntity = GetComponent<LivingEntity>();
        playerEntity = controlledEntity as PlayerEntity;

        if (controlledEntity == null)
            Debug.LogError("PlayerController: LivingEntity component not found!");

        StartCoroutine(WaitForInitialization());
    }

    private System.Collections.IEnumerator WaitForInitialization()
    {
        yield return new WaitForEndOfFrame();
        isInitialized = true;
        Debug.Log("PlayerController initialization complete");
    }

    private void Update()
    {
        if (!isInitialized || controlledEntity == null) return;

        wheelConsumedThisFrame = false;

        HandleMovementInput();
        HandleJumpInput();
        HandleWeaponInput();
        HandleWeaponSwitchInput();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        controlledEntity.Move(horizontal);
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !Input.GetKey(downKey))
            controlledEntity.Jump();

        if (Input.GetKeyDown(jumpKey) && Input.GetKey(downKey))
            controlledEntity.DropThroughPlatform();
    }

    private void HandleWeaponInput()
    {
        if (playerEntity == null) return;

        Vector2 aimDirection = GetAimDirection();

        if (Input.GetMouseButtonDown(0))      playerEntity.HandleWeaponInput(MouseInputType.LeftDown,  aimDirection);
        else if (Input.GetMouseButton(0))     playerEntity.HandleWeaponInput(MouseInputType.LeftHold,  aimDirection);
        else if (Input.GetMouseButtonUp(0))   playerEntity.HandleWeaponInput(MouseInputType.LeftUp,    aimDirection);

        if (Input.GetMouseButtonDown(1))      playerEntity.HandleWeaponInput(MouseInputType.RightDown, aimDirection);
        else if (Input.GetMouseButton(1))     playerEntity.HandleWeaponInput(MouseInputType.RightHold, aimDirection);
        else if (Input.GetMouseButtonUp(1))   playerEntity.HandleWeaponInput(MouseInputType.RightUp,   aimDirection);
    }

    private void HandleWeaponSwitchInput()
    {
        var wm = playerEntity?.WeaponManager;
        if (wm == null) return;

        // 숫자키: 1 → Next, 2 → Prev
        if (Input.GetKeyDown(KeyCode.Alpha1))
            wm.Next();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            wm.Prev();

        // 마우스 휠: 아래(음수) → Next, 위(양수) → Prev
        var scroll = Input.mouseScrollDelta.y;
        if (!wheelConsumedThisFrame && scroll < 0f)
        {
            wm.Next();
            wheelConsumedThisFrame = true;
        }
        else if (!wheelConsumedThisFrame && scroll > 0f)
        {
            wm.Prev();
            wheelConsumedThisFrame = true;
        }
    }

    private Vector2 GetAimDirection()
    {
        if (Camera.main == null)
        {
            Debug.LogWarning("Main camera not found");
            return Vector2.right;
        }

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2 aimDirection = (mouseWorldPos - controlledEntity.transform.position).normalized;
        return aimDirection;
    }
}
