using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "InteractState", menuName = "Spellbound/ActionStates/InteractState")]
    public class InteractStateSO : BaseActionStateSO {
        public override void EnterStateLogic(ActionStateMachine stateMachine) {
            StateMachine = stateMachine;

            StateHelper.NotifyActionStateChange(this);
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() { }

        public override void CheckSwitchStateLogic() { }

        public override void ExitStateLogic() {

        }
    }
}