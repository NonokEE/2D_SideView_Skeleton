// Assets/01_Scripts/Camera/SmoothCameraFollow.cs
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 1, -10);
    public float followSpeed = 3f;        // 1/s → SmoothTime = 1f/followSpeed

    [Header("Dead Zone")]
    public Vector2 deadZone = new Vector2(0f, 0f); // (half width, half height)

    [Header("Stop Tuning")]
    public float velocityEpsilon = 0.01f; // 속도 임계
    public float positionEpsilon = 0.01f; // 위치 임계

    private Vector3 velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 current = transform.position;
        Vector3 targetPos = target.position + offset;

        // Z 고정
        targetPos.z = offset.z;
        current.z = offset.z;

        // Dead Zone 보정: 카메라 기준의 타겟 편차
        Vector3 delta = targetPos - current;
        Vector2 dz = deadZone;

        // Dead Zone 내부라면 이동량을 0으로, 경계 밖이라면 경계까지의 초과분만 이동 목표에 반영
        float dx = Mathf.Abs(delta.x) <= dz.x ? 0f : (delta.x - Mathf.Sign(delta.x) * dz.x);
        float dy = Mathf.Abs(delta.y) <= dz.y ? 0f : (delta.y - Mathf.Sign(delta.y) * dz.y);

        Vector3 desired = current + new Vector3(dx, dy, 0f);

        // 항상 SmoothDamp로 감쇠 이동(정지도 부드럽게)
        float smoothTime = 1f / Mathf.Max(0.0001f, followSpeed);
        Vector3 next = Vector3.SmoothDamp(current, desired, ref velocity, smoothTime); // 감쇠 이동 [19]

        // 히스테리시스: 속도/오차가 매우 작으면 완전히 정지
        if ((desired - next).sqrMagnitude < (positionEpsilon * positionEpsilon) &&
            velocity.sqrMagnitude < (velocityEpsilon * velocityEpsilon))
        {
            next = desired;
            velocity = Vector3.zero;
        }

        transform.position = next;
    }
}
