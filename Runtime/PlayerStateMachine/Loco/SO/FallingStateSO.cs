using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "FallingState", menuName = "Spellbound/LocoStates/FallingState")]
    public class FallingStateSO : BaseLocoStateSO {
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Falling);
        }
        
        public override void UpdateStateLogic(in LocoStateContext ctx) {
            CheckSwitchStateLogic(in ctx);
        }
        
        public override void FixedUpdateStateLogic(in LocoStateContext ctx) {
            
        }
        
        public override void CheckSwitchStateLogic(in LocoStateContext ctx) {
            if (ctx.Grounded)
                StateMachine.ChangeState(StateMachine.LandingStateDriver);
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}