// Assets/01_Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode downKey = KeyCode.S;

    private LivingEntity controlledEntity;
    private PlayerEntity playerEntity;
    private bool isInitialized = false;
    
    private void Start()
    {
        controlledEntity = GetComponent<LivingEntity>();
        playerEntity = controlledEntity as PlayerEntity;
        
        if (controlledEntity == null)
        {
            Debug.LogError("PlayerController: LivingEntity component not found!");
        }
        
        // 초기화 완료 대기
        StartCoroutine(WaitForInitialization());
    }

    private System.Collections.IEnumerator WaitForInitialization()
    {
        // PlayerEntity의 Initialize()가 완료될 때까지 대기
        yield return new WaitForEndOfFrame();
        isInitialized = true;
        Debug.Log("PlayerController initialization complete");
    }

    private void Update()
    {
        if (!isInitialized || controlledEntity == null) return;
        
        HandleMovementInput();
        HandleJumpInput();
        HandleWeaponInput();
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
        if (playerEntity == null) return;
        
        Vector2 aimDirection = GetAimDirection();
        
        // 좌클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"Left click detected, aim direction: {aimDirection}");
            playerEntity.HandleWeaponInput(MouseInputType.LeftDown, aimDirection);
        }
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
