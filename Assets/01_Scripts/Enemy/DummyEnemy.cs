using UnityEngine;

public class DummyEnemy : LivingEntity
{
    [Header("DummyEnemy Specific")]
    [SerializeField] private BasicEnemyAnimationHandler animationHandler;
    
    protected override void Awake()
    {
        base.Awake();

        // AnimationHandler 자동 할당
        if (animationHandler == null)
            animationHandler = GetComponent<BasicEnemyAnimationHandler>();
    }

    public override void Initialize()
    {
        // 이동 비활성화 (고정형)
        SetCanMove(false);
        SetCanJump(false);
    }

    // 피격 시 애니메이션 처리
    protected override void OnDamageTaken(DamageData damageData)
    {
        base.OnDamageTaken(damageData);
        
        // 피격 애니메이션 실행
        if (animationHandler != null)
        {
            animationHandler.PlayHitEffect();
        }
        
        Debug.Log($"DummyEnemy took damage and played hit effect");
    }
    
    // DummyEnemy 전용 무적시간 계산
    protected override float CalculateHitInvincibilityDuration(DamageData damageData)
    {
        return 1.0f; // 테스트용 짧은 무적시간
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