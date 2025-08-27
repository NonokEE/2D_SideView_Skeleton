using UnityEngine;
using Combat.Projectiles;

public static class BulletStrategyFactory
{
    // Movement 전략만 유지
    public static IMovementStrategy CreateMovementStrategy(MovementType type)
    {
        return type switch
        {
            MovementType.Straight => new StraightMovementStrategy(),
            MovementType.Homing => new HomingMovementStrategy(),
            MovementType.Sine => new SineMovementStrategy(),
            MovementType.Spiral => new SpiralMovementStrategy(),
            MovementType.Curve => new CurveMovementStrategy(),
            MovementType.Gravity => new GravityMovementStrategy(),
            _ => new StraightMovementStrategy()
        };
    }

    // Lifetime 전략 유지
    public static ILifetimeStrategy CreateLifetimeStrategy(LifetimeType type)
    {
        return type switch
        {
            LifetimeType.Time => new TimeLifetimeStrategy(),
            LifetimeType.Distance => new DistanceLifetimeStrategy(),
            LifetimeType.HitCount => new HitCountLifetimeStrategy(),
            LifetimeType.Infinite => new InfiniteLifetimeStrategy(),
            _ => new TimeLifetimeStrategy()
        };
    }

    // ⚠️ 제거된 메서드들 (CollisionAction으로 대체)
    // CreateCollisionStrategy() - 삭제됨
    // CreateEffectStrategy() - 삭제됨
}
