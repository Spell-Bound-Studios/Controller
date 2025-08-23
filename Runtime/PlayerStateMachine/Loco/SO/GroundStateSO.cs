using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "GroundState", menuName = "Spellbound/LocoStates/GroundState")]
    public class GroundStateSO : BaseLocoStateSO {
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Grounded);
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
            StateHelper.NotifyLocoAnimationSpeedChange(Cc.HorizontalSpeed);
        }
        
        public override void FixedUpdateStateLogic() {
            Cc.HandleHorizontalVelocityInput();
        }
        
        public override void CheckSwitchStateLogic() {
            if (!Cc.GroundFlag)
                StateMachine.ChangeState(StateMachine.FallingStateDriver);
            
            if (Cc.JumpFlag)
                StateMachine.ChangeState(StateMachine.JumpingStateDriver);
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}