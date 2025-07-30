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
}
