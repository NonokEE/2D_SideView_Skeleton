using UnityEngine;
using System;

public class TimeLifetimeStrategy : ILifetimeStrategy
{
    private float maxLifetime;
    private float currentLifetime;
    private Action onLifetimeEnd;
    
    public bool ShouldDestroy => currentLifetime >= maxLifetime;

    public void Initialize(BulletPhysicsConfig config, Action onEnd)
    {
        maxLifetime = config.MaxLifetime;
        currentLifetime = 0f;
        onLifetimeEnd = onEnd;
        Debug.Log($"TimeLifetime initialized: {maxLifetime}s");
    }

    public void UpdateLifetime(float deltaTime, float traveledDistance, int hitCount)
    {
        currentLifetime += deltaTime;
        if (ShouldDestroy && onLifetimeEnd != null)
        {
            Debug.Log($"Lifetime expired: {currentLifetime}/{maxLifetime}");
            onLifetimeEnd.Invoke();
        }
    }
}