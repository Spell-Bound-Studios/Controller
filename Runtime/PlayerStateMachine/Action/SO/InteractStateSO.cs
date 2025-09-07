using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "InteractState", menuName = "Spellbound/ActionStates/InteractState")]
    public class InteractStateSO : BaseActionStateSO {
        private readonly WaitForSeconds _duration = new(0.1f);
        private Coroutine _routine;
        
        public override void EnterStateLogic(ActionStateMachine stateMachine) {
            StateMachine = stateMachine;

            StateHelper.NotifyActionStateChange(this);
            
            _routine = StateMachine.CharController.StartCoroutine(Routine());
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() { }

        public override void CheckSwitchStateLogic() {
            if (_routine == null)
                StateMachine.ChangeState(StateMachine.ReadyStateDriver);
        }

        public override void ExitStateLogic() {
            //Cc.interactFlagged = false;
        }
        
        private IEnumerator Routine() {
            // Prevents immediately being able to jump again.
            yield return _duration;
            _routine = null;
        }
    }
}