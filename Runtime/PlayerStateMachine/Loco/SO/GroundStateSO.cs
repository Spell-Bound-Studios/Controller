using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "GroundState", menuName = "Spellbound/LocoStates/GroundState")]
    public class GroundStateSO : BaseLocoStateSO {
        private const float HSpeedModifier = 1f;
        private const float VSpeedModifer = 1f;
        
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Grounded);
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            Cc.HandleInput(HSpeedModifier, VSpeedModifer);
            StateHelper.NotifyLocoAnimationSpeedChange(Cc.horizontalSpeed);
        }
        
        public override void CheckSwitchStateLogic() {
            if (!Cc.GroundFlag)
                StateMachine.ChangeState(StateMachine.FallingStateDriver);
            
            if (Cc.jumpFlag)
                StateMachine.ChangeState(StateMachine.JumpingStateDriver);
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}