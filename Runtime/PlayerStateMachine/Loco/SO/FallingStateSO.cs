using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "FallingState", menuName = "Spellbound/LocoStates/FallingState")]
    public class FallingStateSO : BaseLocoStateSO {
        public override void Initialize() {
            
        }
        
        public override void EnterStateLogic() {
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Falling);
        }
        
        public override void UpdateStateLogic(in LocoStateContext ctx) {
            CheckSwitchStateLogic(in ctx);
        }
        
        public override void FixedUpdateStateLogic(in LocoStateContext ctx) {
            StateHelper.NotifyLocoAnimationSpeedChange(ctx.Speed);
        }
        
        public override void CheckSwitchStateLogic(in LocoStateContext ctx) {
            
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}