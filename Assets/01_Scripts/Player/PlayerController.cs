// Assets/01_Scripts/Player/PlayerController.cs
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode jumpKey = KeyCode.Space;
    
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
        if (Input.GetKeyDown(jumpKey))
        {
            controlledEntity.Jump();
        }
    }
}
