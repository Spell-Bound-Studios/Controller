using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "JumpingState", menuName = "Spellbound/LocoStates/JumpingState")]
    public class JumpingStateSO : BaseLocoStateSO {
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Jumping);
        }
        
        public override void UpdateStateLogic(in LocoStateContext ctx) {
            CheckSwitchStateLogic(in ctx);
        }
        
        public override void FixedUpdateStateLogic(in LocoStateContext ctx) {

        }
        
        public override void CheckSwitchStateLogic(in LocoStateContext ctx) {
            
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}