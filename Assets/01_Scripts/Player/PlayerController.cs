// Assets/01_Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode downKey = KeyCode.S;

    private LivingEntity controlledEntity;

    private void Start()
    {
        controlledEntity = GetComponent<LivingEntity>();
        if (controlledEntity == null)
        {
            Debug.LogError("PlayerController: LivingEntity component not found!");
        }
    }

    private void Update()
    {
        if (controlledEntity == null) return;

        HandleMovementInput();
        HandleJumpInput();
        HandleWeaponInput(); // 새로 추가
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        controlledEntity.Move(horizontal);
    }

    private void HandleJumpInput()
    {
        // 일반 점프
        if (Input.GetKeyDown(jumpKey) && !Input.GetKey(downKey))
        {
            controlledEntity.Jump();
        }

        // 플랫폼 하강 (아래키+점프 동시)
        if (Input.GetKeyDown(jumpKey) && Input.GetKey(downKey))
        {
            controlledEntity.DropThroughPlatform();
        }
    }
    
    private void HandleWeaponInput()
    {
        PlayerEntity playerEntity = controlledEntity as PlayerEntity;
        if (playerEntity == null) return;
        
        // 마우스 방향 계산
        Vector2 aimDirection = GetAimDirection();
        
        // 좌클릭 처리
        if (Input.GetMouseButtonDown(0))
            playerEntity.HandleWeaponInput(MouseInputType.LeftDown, aimDirection);
        else if (Input.GetMouseButton(0))
            playerEntity.HandleWeaponInput(MouseInputType.LeftHold, aimDirection);
        else if (Input.GetMouseButtonUp(0))
            playerEntity.HandleWeaponInput(MouseInputType.LeftUp, aimDirection);
        
        // 우클릭 처리
        if (Input.GetMouseButtonDown(1))
            playerEntity.HandleWeaponInput(MouseInputType.RightDown, aimDirection);
        else if (Input.GetMouseButton(1))
            playerEntity.HandleWeaponInput(MouseInputType.RightHold, aimDirection);
        else if (Input.GetMouseButtonUp(1))
            playerEntity.HandleWeaponInput(MouseInputType.RightUp, aimDirection);
    }

    private Vector2 GetAimDirection()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        Vector2 aimDirection = (mouseWorldPos - controlledEntity.transform.position).normalized;
        return aimDirection;
    }
}
