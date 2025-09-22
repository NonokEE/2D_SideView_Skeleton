using UnityEngine;
using System;

public interface ILifetimeStrategy
{
    void Initialize(BulletPhysicsConfig config, Action onLifetimeEnd);
    void UpdateLifetime(float deltaTime, float traveledDistance, int hitCount);
    bool ShouldDestroy { get; }
}
