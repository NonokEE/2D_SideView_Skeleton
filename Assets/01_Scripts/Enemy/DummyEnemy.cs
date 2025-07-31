using UnityEngine;

public class DummyEnemy : LivingEntity
{
    public override void Initialize()
    {
        // 이동 비활성화 (고정형)
        SetCanMove(false);
        SetCanJump(false);
    }


    protected override void OnDie()
    {
        // DummyEnemy 사망 처리
        gameObject.SetActive(false);
    }

    // 이동 관련 메서드 오버라이드 (비활성화)
    public override void Move(float horizontal) { }
    public override void Jump() { }
    public override void Attack() { }

    // 디버그
    [ContextMenu("TakeDamage")]
    public void TakeDamageFromContextMenu()
    {
        TakeDamage(50);
    }
    
    private void OnGUI()
    {
        if (!IsAlive) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
        screenPos.y = Screen.height - screenPos.y;

        GUI.Label(new Rect(screenPos.x - 50, screenPos.y, 100, 20),
                  $"HP: {CurrentHealth}/{MaxHealth}");
    }
}