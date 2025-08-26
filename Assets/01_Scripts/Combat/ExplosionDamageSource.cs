using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 폭발 데미지 소스. 생성 즉시 폭발하여 범위 내 Entity에게 데미지를 가함.
/// CapsuleCollider2D를 사용하여 다양한 폭발 형태 지원 (원형, 사각형, 긴 폭발 등).
/// ScaleStretch + ColorBlink 애니메이션으로 시각적 폭발 효과 제공.
/// </summary>
public class ExplosionDamageSource : DamageSourceEntity
{
    #region Inspector Fields
    
    [Header("Explosion Settings")]
    [SerializeField] private Vector2 explosionSize = new Vector2(3f, 3f);
    [SerializeField] private float explosionDuration = 0.3f;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showExplosionGizmos = true;
    
    #endregion
    
    #region Private Fields
    
    private bool hasExploded = false;
    private HashSet<BaseEntity> damagedEntities = new HashSet<BaseEntity>();
    
    #endregion
    
    #region Initialization
    
    public override void Initialize()
    {
        if (!hasExploded)
        {
            StartCoroutine(ExplodeCoroutine());
        }
    }
    
    public override void Initialize(BaseEntity sourceOwner, BaseWeapon sourceWeapon)
    {
        base.Initialize(sourceOwner, sourceWeapon);
        SetupExplosionCollider();
        Initialize();
    }
    
    /// <summary>
    /// 폭발 범위에 맞게 CapsuleCollider2D 설정
    /// </summary>
    private void SetupExplosionCollider()
    {
        if (physicsContainer?.MainCollider is CapsuleCollider2D capsuleCollider)
        {
            capsuleCollider.size = explosionSize;
            capsuleCollider.direction = CapsuleDirection2D.Vertical;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: MainCollider is not CapsuleCollider2D!");
        }
    }
    
    #endregion
    
    #region Explosion Logic
    
    /// <summary>
    /// 폭발 실행 코루틴. 초기화 지연 → 시각 효과 → 지속시간 대기 → 정리
    /// </summary>
    private IEnumerator ExplodeCoroutine()
    {
        // 초기화 지연 (컴포넌트 Awake 완료 보장)
        yield return null;
        
        hasExploded = true;
        PlayExplosionAnimation();
        
        // 폭발 지속시간 대기
        yield return new WaitForSeconds(explosionDuration);
        
        // 폭발 정리
        CleanupExplosion();
    }
    
    /// <summary>
    /// 폭발 시각 효과 실행 (ScaleStretch + ColorBlink)
    /// </summary>
    private void PlayExplosionAnimation()
    {
        if (visualContainer != null)
        {
            visualContainer.PlayGeometricAnimation(0); // ScaleStretch
            visualContainer.PlayGeometricAnimation(1); // ColorBlink
        }
    }
    
    /// <summary>
    /// 폭발 정리 및 비활성화
    /// </summary>
    private void CleanupExplosion()
    {
        StopAllCoroutines();
        gameObject.SetActive(false);
    }
    
    #endregion
    
    #region Damage Handling
    
    /// <summary>
    /// 트리거 충돌 시 데미지 처리. 동일 Entity에게 중복 데미지 방지.
    /// </summary>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!hasExploded) return;
        
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (IsValidDamageTarget(target))
        {
            ApplyExplosionDamage(target);
        }
    }
    
    /// <summary>
    /// 유효한 데미지 대상인지 확인
    /// </summary>
    private bool IsValidDamageTarget(BaseEntity target)
    {
        return target != null && 
               CanDamage(target) && 
               !damagedEntities.Contains(target);
    }
    
    /// <summary>
    /// 폭발 데미지 적용 및 기록
    /// </summary>
    private void ApplyExplosionDamage(BaseEntity target)
    {
        damagedEntities.Add(target);
        
        DamageData damageData = GenerateDamageData(target);
        target.TakeDamage(damageData);
        
        Debug.Log($"Explosion hit {target.entityID} for {damage} damage");
    }
    
    #endregion
    
    #region Debug Visualization
    
    /// <summary>
    /// 런타임 중 폭발 범위 시각화
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showExplosionGizmos) return;
        
        Vector3 worldPosition = transform.position;
        
        if (Application.isPlaying && hasExploded)
        {
            DrawActiveExplosionGizmo(worldPosition);
        }
        else
        {
            DrawInactiveExplosionGizmo(worldPosition);
        }
    }
    
    /// <summary>
    /// 폭발 중 활성 상태 Gizmo (빨간색)
    /// </summary>
    private void DrawActiveExplosionGizmo(Vector3 position)
    {
        // 폭발 판정 영역 (빨간색 테두리)
        Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
        Gizmos.DrawWireCube(position, explosionSize);
        
        // 내부 채우기 (반투명 빨간색)
        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
        Gizmos.DrawCube(position, explosionSize);
    }
    
    /// <summary>
    /// 폭발 전 예상 범위 Gizmo (주황색)
    /// </summary>
    private void DrawInactiveExplosionGizmo(Vector3 position)
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireCube(position, explosionSize);
    }
    
    /// <summary>
    /// 선택된 상태에서 상세 정보 표시
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!showExplosionGizmos) return;
        
        Vector3 worldPosition = transform.position;
        
        // 폭발 판정 영역 (파란색 테두리)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(worldPosition, explosionSize);
        
        // 판정 영역 강조 (반투명 파란색)
        Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        Gizmos.DrawCube(worldPosition, explosionSize);
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// 폭발 크기 설정 (런타임 동적 변경 가능)
    /// </summary>
    public void SetExplosionSize(Vector2 newSize)
    {
        explosionSize = newSize;
        SetupExplosionCollider();
    }
    
    /// <summary>
    /// 폭발 지속시간 설정
    /// </summary>
    public void SetExplosionDuration(float duration)
    {
        explosionDuration = Mathf.Max(0.1f, duration);
    }
    
    /// <summary>
    /// 현재 폭발 상태 확인
    /// </summary>
    public bool HasExploded => hasExploded;
    
    /// <summary>
    /// 데미지를 입은 Entity 수 확인
    /// </summary>
    public int DamagedEntityCount => damagedEntities.Count;
    
    #endregion
}
