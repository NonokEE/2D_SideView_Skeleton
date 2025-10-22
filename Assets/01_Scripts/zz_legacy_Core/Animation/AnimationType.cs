public enum GeometricAnimationType
{
    // 크기 변형
    ScaleStretch,      // 점프 시 세로 늘어남
    ScaleSqueeze,      // 착지 시 가로 넓어짐
    ScalePulse,        // 맥박 효과
    
    // 회전 변형
    RotateWiggle,      // 좌우 흔들림
    RotateSpin,        // 회전
    RotateFlip,        // 뒤집기
    
    // 위치 변형
    Bounce,            // 통통 튀기
    Shake,             // 진동
    Float,             // 부유
    
    // 색상 변형
    ColorBlink,        // 깜빡임
    ColorFade,         // 투명도 변화
    ColorShift,        // 색상 변화
    
    // 복합 효과
    HitReaction,       // 피격 반응
    DeathDissolve,     // 사망 효과
    SpawnEffect        // 생성 효과
}
