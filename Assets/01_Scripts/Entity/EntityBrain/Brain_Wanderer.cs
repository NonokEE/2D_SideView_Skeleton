using UnityEngine;

namespace Entity.Controller
{
    public class Brain_Wanderer : Brain_Base<ISynapse_Wanderer, WandererAction>
    {
        protected override void Initialize()
        {
            wantingAction = WandererAction.Idle;
            currentAction = WandererAction.Idle;
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

    }

    public interface ISynapse_Wanderer : ISynapse_Base
    {
        public void Wander();
    }
    public enum WandererAction
    {
        Idle,
        Wander
    }
}