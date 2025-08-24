using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "FallingState", menuName = "Spellbound/LocoStates/FallingState")]
    public class FallingStateSO : BaseLocoStateSO {
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Falling);
            
            Debug.Log("FallingStateSO EnterStateLogic");
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            Cc.HandleHorizontalVelocityInput();
        }
        
        public override void CheckSwitchStateLogic() {
            if (Cc.GroundFlag)
                StateMachine.ChangeState(StateMachine.LandingStateDriver);
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}