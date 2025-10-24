using UnityEngine;

namespace Entity.Controller
{
    public class Brain_Trooper : Brain_Base<ISynapse_Trooper, TrooperAction>
    {
        /* Fields for Think*/
        // TODO Think에 필요한 변수들 선언

        /* Abstract Methods Implementation */
        protected override void Initialize()
        {
            wantingAction = TrooperAction.Idle;
            currentAction = TrooperAction.Idle;
        }

        protected override void See()
        {
            // TODO Think에 필요한 변수들 갱신
        }

        protected override void Think()
        {
            switch (currentAction)
            {
                case TrooperAction.Idle:

                    break;

                case TrooperAction.Wander:

                    break;

                case TrooperAction.ChaseTarget:

                    break;

                case TrooperAction.AttackTarget:

                    break;
            }
        }

        /* Action Handlers */

        private void WantToWander() { }
        private bool CanWander() { return false; }

        private void WantToChaseTarget() { }
        private bool CanChaseTarget() { return false; }

        private void WantToAttackTarget() {}
        private bool CanAttackTarget() { return false; }

    }

    public interface ISynapse_Trooper : ISynapse_Base
    {
        public void Wander();
        public void ChaseTarget();
        public void AttackTarget();
    }

    public enum TrooperAction
    {
        Idle,
        Wander,
        ChaseTarget,
        AttackTarget,
    }
}