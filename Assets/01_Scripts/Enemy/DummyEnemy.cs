using UnityEngine;

public class DummyEnemy : LivingEntity
{
    [Header("Dummy Enemy Settings")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public override void Initialize()
    {
        currentHealth = maxHealth;
        entityID = "DummyEnemy_01";

        // 이동 비활성화 (고정형)
        SetCanMove(false);
        SetCanJump(false);

        // 빨간색 설정
        SetColor(Color.red);
    }

    [ContextMenu("TakeDamage")]
    public void TakeDamageFromContextMenu()
    {
        TakeDamage(50);
    }
    public override void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"DummyEnemy took {damage} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("DummyEnemy died!");
        gameObject.SetActive(false);
    }

    // 이동 관련 메서드 오버라이드 (비활성화)
    public override void Move(float horizontal) { }
    public override void Jump() { }
    public override void Attack() { }

    private void OnGUI()
    {
        if (!isActive) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 1.5f);
        screenPos.y = Screen.height - screenPos.y; // Y축 반전
        
        GUI.Label(new Rect(screenPos.x - 50, screenPos.y, 100, 20), 
                $"HP: {currentHealth}/{maxHealth}");
    }
}
