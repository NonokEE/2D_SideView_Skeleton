using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerEntity))]
public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode downKey = KeyCode.S;

    private PlayerEntity playerEntity;
    private bool wheelConsumedThisFrame = false;

    private IEnumerator Start()
    {
        yield return null; 
        playerEntity = GetComponent<PlayerEntity>();

        if (playerEntity == null)
            Debug.LogError("PlayerController: PlayerEntity component not found!");
    }


    private void Update()
    {
        if (playerEntity == null) return;

        wheelConsumedThisFrame = false;

        HandleMovementInput();
        HandleJumpInput();
        HandleWeaponInput();
        HandleWeaponSwitchInput();
    }

    private void HandleMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        playerEntity.Move(horizontal);
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && !Input.GetKey(downKey))
            playerEntity.Jump();

        if (Input.GetKeyDown(jumpKey) && Input.GetKey(downKey))
            playerEntity.IgnorePlatform();
    }

    private void HandleWeaponInput()
    {
        if (playerEntity == null) return;

        Vector2 aimDirection = GetAimDirection();

             if(Mouse.current.leftButton.wasPressedThisFrame)  playerEntity.HandleWeaponInput(MouseInputType.LeftDown, aimDirection);
        else if(Mouse.current.leftButton.isPressed)            playerEntity.HandleWeaponInput(MouseInputType.LeftHold, aimDirection);
        else if(Mouse.current.leftButton.wasReleasedThisFrame) playerEntity.HandleWeaponInput(MouseInputType.LeftUp, aimDirection);

             if(Mouse.current.rightButton.wasPressedThisFrame)  playerEntity.HandleWeaponInput(MouseInputType.RightDown, aimDirection);
        else if(Mouse.current.rightButton.isPressed)            playerEntity.HandleWeaponInput(MouseInputType.RightHold, aimDirection);
        else if(Mouse.current.rightButton.wasReleasedThisFrame) playerEntity.HandleWeaponInput(MouseInputType.RightUp, aimDirection);

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
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main camera not found");
            return Vector2.right;
        }

        // 플레이어가 서있는 평면(카메라 전방에 수직)과 마우스 광선의 교차점 계산
        Vector3 playerPos = playerEntity.transform.position;
        Plane plane = new Plane(-cam.transform.forward, playerPos);
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            Vector2 dir = ((Vector2)(hit - playerPos));
            if (dir.sqrMagnitude > 0.0001f) return dir.normalized;
        }

        // 교차 실패 폴백(카메라-플레인 기하가 비정상일 때)
        return Vector2.right;
    }
}
