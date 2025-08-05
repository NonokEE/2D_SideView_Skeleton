using UnityEngine;

[System.Serializable]
public enum InvincibilityType
{
    None = 0,
    HitInvincibility = 1,     // 피격 무적 (우선순위 1)
    BuffInvincibility = 2,    // 버프 무적 (우선순위 2)
    CutsceneInvincibility = 3, // 연출 무적 (우선순위 3)
    SpawnInvincibility = 4     // 스폰 무적 (우선순위 4)
}

[System.Serializable]
public class InvincibilityData
{
    public InvincibilityType type;
    public float remainingTime;
    public float totalTime;
    public bool showVisualEffect;
    
    public InvincibilityData(InvincibilityType invType, float duration, bool visual = true)
    {
        type = invType;
        remainingTime = duration;
        totalTime = duration;
        showVisualEffect = visual;
    }
}

public static class InvincibilityHelper
{
    // 무적 타입별 우선순위 (높을수록 우선)
    public static int GetPriority(InvincibilityType type)
    {
        return (int)type;
    }
    
    // 무적 타입별 시각적 효과 색상
    public static Color GetEffectColor(InvincibilityType type)
    {
        switch (type)
        {
            case InvincibilityType.HitInvincibility:
                return Color.red;
            case InvincibilityType.BuffInvincibility:
                return Color.yellow;
            case InvincibilityType.CutsceneInvincibility:
                return Color.clear; // 시각적 효과 없음
            case InvincibilityType.SpawnInvincibility:
                return Color.cyan;
            default:
                return Color.white;
        }
    }
    
    // 무적 타입별 깜빡임 간격
    public static float GetBlinkInterval(InvincibilityType type)
    {
        switch (type)
        {
            case InvincibilityType.HitInvincibility:
                return 0.1f;
            case InvincibilityType.BuffInvincibility:
                return 0.3f;
            case InvincibilityType.SpawnInvincibility:
                return 0.2f;
            default:
                return 0.1f;
        }
    }
}
