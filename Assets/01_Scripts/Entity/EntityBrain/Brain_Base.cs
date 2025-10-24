using UnityEngine;

namespace Entity.Controller
{
    /// <summary>
    /// 인공지능의 뇌. 주어진 상황을 판별하여 의사를 결정하고 상태를 제어, 시냅스 인터페이스를 에게"의사"에 해당하는 함수를 호출함.
    /// </summary>
    public abstract class Brain_Base<TSynapse, TAction> : MonoBehaviour 
        where TSynapse : ISynapse_Base 
        where TAction : System.Enum
    {
        /* Base Fields*/
        [SerializeField] protected BaseEntity target = null;

        [Space]
        [SerializeField] protected TAction wantingAction;
        [SerializeField] protected TAction currentAction;

        [Space]
        [SerializeField] protected TSynapse Synapse;
        
        /* Unity Methods */
        private void Awake()
        {
            target = null;
            Synapse = GetComponent<TSynapse>();

            Initialize();
        }

        private void Update()
        {
            See();
            Think();
        }

        /* Abstract Methods */
        /// <summary>
        /// 초기화 필수 항목 : wantingAction, currentAction
        /// </summary>
        protected abstract void Initialize();

        /// <summary>
        /// Brain의 판단 기준이 되는 변수들을 갱신
        /// </summary>
        protected abstract void See();

        /// <summary>
        /// Brain의 사고 로직을 구현
        /// </summary>
        protected abstract void Think();
    }

    public interface ISynapse_Base
    {
        
    }
}