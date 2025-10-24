using UnityEngine;

namespace Entity.Body
{
    public class Body_Base : LivingEntity, IControllable
    {
        public GameObject GameObject => this.gameObject;
        public Transform Transform => this.transform;
    }
}