using UnityEngine;
using Combat.Projectiles;

public static class BulletStrategyFactory
{
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

    public static ICollisionStrategy CreateCollisionStrategy(HitBehavior behavior)
    {
        return behavior switch
        {
            HitBehavior.Stop => new StopCollisionStrategy(),
            HitBehavior.Pierce => new PierceCollisionStrategy(),
            HitBehavior.Bounce => new BounceCollisionStrategy(),
            HitBehavior.Explode => new ExplodeCollisionStrategy(),
            _ => new StopCollisionStrategy()
        };
    }

    public static IEffectStrategy CreateEffectStrategy(DeathEffectType type)
    {
        return type switch
        {
            DeathEffectType.None => new NoEffectStrategy(),
            DeathEffectType.Explode => new ExplodeEffectStrategy(),
            DeathEffectType.PoisonCloud => new PoisonCloudEffectStrategy(),
            DeathEffectType.Split => new SplitEffectStrategy(),
            _ => new NoEffectStrategy()
        };
    }
}
