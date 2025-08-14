using UnityEngine;

public interface IEffectStrategy
{
    void ExecuteEffect(Vector3 position, BulletPhysicsConfig config, BaseEntity owner);
}
