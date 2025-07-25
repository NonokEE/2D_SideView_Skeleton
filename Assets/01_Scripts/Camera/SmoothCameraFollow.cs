using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 1, -10);
    public float followSpeed = 3f;
    
    [Header("Dead Zone")]
    public Vector2 deadZone = new Vector2(0f, 0f);
    
    private Vector3 velocity = Vector3.zero;
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 targetPosition = target.position + offset;
        Vector3 currentPosition = transform.position;
        
        // Dead Zone 계산
        Vector3 difference = targetPosition - currentPosition;
        difference.z = 0; // 2D에서는 Z축 무시
        
        if (Mathf.Abs(difference.x) > deadZone.x || Mathf.Abs(difference.y) > deadZone.y)
        {
            Vector3 desiredPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, 1f / followSpeed);
            desiredPosition.z = offset.z; // Z축 고정
            transform.position = desiredPosition;
        }
    }
}
