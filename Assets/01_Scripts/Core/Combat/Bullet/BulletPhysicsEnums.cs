using UnityEngine;

namespace Combat.Projectiles
{
    [System.Serializable]
    public enum MovementType
    {
        Straight,       // 직선 이동
        Curve,          // 곡선 이동
        Homing,         // 유도 이동
        Gravity,        // 중력 영향
        Sine,           // 사인파 이동
        Spiral          // 나선 이동
    }

    [System.Serializable]
    public enum LifetimeType
    {
        Time,           // 시간 기반
        Distance,       // 거리 기반
        HitCount,       // 충돌 횟수 기반
        Infinite        // 무한 (조건부 소멸만)
    }

    [System.Serializable]
    public enum DeathEffectType
    {
        None,           // 효과 없음
        Explode,        // 폭발
        PoisonCloud,    // 독구름
        Summon,         // 소환
        Split,          // 분열
        Flash           // 섬광
    }

    [System.Serializable]
    public enum BulletCapsuleDirection
    {
        Vertical,       // 세로 방향
        Horizontal      // 가로 방향
    }

    [System.Serializable]
    public enum CollisionActionType
    {
        Penetrate,      // 관통
        Bounce,         // 반사  
        Stop,           // 정지
        Destroy,        // 파괴
        SpawnEntity     // Entity 생성
    }
}
