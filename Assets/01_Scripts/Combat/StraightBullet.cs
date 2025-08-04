using UnityEngine;

public class StraightBullet : ProjectileDamageSource
{
    public override void Initialize()
    {
        // Trigger 콜라이더 설정
        if (physicsContainer != null && physicsContainer.MainCollider != null)
        {
            physicsContainer.MainCollider.isTrigger = true;
        }
    }

}
