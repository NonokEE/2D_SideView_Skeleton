using UnityEngine;
using System;

public class HitCountLifetimeStrategy : ILifetimeStrategy
{
    public bool ShouldDestroy => false;
    
    public void Initialize(BulletPhysicsConfig config, Action onEnd) { }
    public void UpdateLifetime(float deltaTime, float traveledDistance, int hitCount) { }
}
