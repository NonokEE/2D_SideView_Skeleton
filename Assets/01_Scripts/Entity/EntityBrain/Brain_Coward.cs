using UnityEngine;

namespace Entity.Controller
{
    public class Brain_Coward : Brain_Base<ISynapse_Coward, CowardAction>
    {
        protected override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        protected override void See()
        {
            throw new System.NotImplementedException();
        }

        protected override void Think()
        {
            throw new System.NotImplementedException();
        }

        private void WantToWander() { }
        private bool CanWander() { return false; }
        private void WantToFleeFromTarget() { }
        private bool CanFleeFromTarget() { return false; }
    }

    public interface ISynapse_Coward : ISynapse_Base
    {
        public void Wander();
        public void FleeFromTarget();
    }

    public enum CowardAction
    {
        Idle,
        Wander,
        FleeFromTarget
    }
}