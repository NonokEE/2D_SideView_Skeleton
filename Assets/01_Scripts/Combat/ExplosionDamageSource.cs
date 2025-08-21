using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExplosionDamageSource : DamageSourceEntity
{
    [Header("Explosion Settings")]
    [SerializeField] private Vector2 explosionSize = new Vector2(3f, 3f);
    [SerializeField] private CapsuleDirection2D explosionDirection = CapsuleDirection2D.Horizontal; // 기본 가로 방향
    [SerializeField] private float explosionDuration = 0.3f;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showExplosionGizmos = true;
    
    private bool hasExploded = false;
    private HashSet<BaseEntity> damagedEntities = new HashSet<BaseEntity>();
    
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
        
        if (physicsContainer?.MainCollider is CapsuleCollider2D capsuleCollider)
        {
            capsuleCollider.size = explosionSize;
            capsuleCollider.direction = explosionDirection; // 기본 세로 방향
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: MainCollider is not CapsuleCollider2D!");
        }
        
        Initialize();
    }
    
    private IEnumerator ExplodeCoroutine()
    {
        // ✅ 초기화 지연을 위한 한 프레임 대기
        yield return null;
        
        hasExploded = true;
        
        if (visualContainer != null)
        {
            // 폭발 시각 효과
            visualContainer.PlayGeometricAnimation(0); // ScaleStretch
            visualContainer.PlayGeometricAnimation(1); // ColorBlink
        }
        
        // 폭발 지속시간 대기
        yield return new WaitForSeconds(explosionDuration);
        
        // 폭발 정리
        gameObject.SetActive(false);
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!hasExploded) return;
        
        BaseEntity target = other.GetComponent<BaseEntity>();
        if (target != null && CanDamage(target) && !damagedEntities.Contains(target))
        {
            damagedEntities.Add(target);
            
            DamageData damageData = GenerateDamageData(target);
            target.TakeDamage(damageData);
            
            Debug.Log($"Explosion hit {target.entityID} for {damage} damage");
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showExplosionGizmos) return;
        
        Vector3 worldPosition = transform.position;
        
        if (Application.isPlaying && hasExploded)
        {
            // 폭발 중일 때는 빨간색으로 실제 판정 영역 표시
            Gizmos.color = new Color(1f, 0f, 0f, 0.7f);
            Gizmos.DrawWireCube(worldPosition, explosionSize);
            
            // 내부 채우기 (반투명)
            Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
            Gizmos.DrawCube(worldPosition, explosionSize);
        }
        else
        {
            // 폭발 전에는 주황색으로 예상 영역 표시
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
            Gizmos.DrawWireCube(worldPosition, explosionSize);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showExplosionGizmos) return;
        
        // 선택된 상태에서는 더 자세한 정보 표시
        Vector3 worldPosition = transform.position;
        
        // 폭발 판정 영역 (파란색 테두리)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(worldPosition, explosionSize);
        
        // 판정 영역 라벨 표시를 위한 추가 시각화
        Gizmos.color = new Color(0f, 0f, 1f, 0.1f);
        Gizmos.DrawCube(worldPosition, explosionSize);
    }
}
