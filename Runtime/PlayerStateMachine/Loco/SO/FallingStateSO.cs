using SpellBound.Controller.ManagersAndStatics;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "FallingState", menuName = "Spellbound/LocoStates/FallingState")]
    public class FallingStateSO : BaseLocoStateSO {
        private const float HSpeedModifier = 1f;
        private const float VSpeedModifer = 1f;
        
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Falling);
            
            Cc.SetSensorRange(ControllerHelper.RaycastLength.Extended);
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            Cc.HandleInput(HSpeedModifier, VSpeedModifer);
        }
        
        public override void CheckSwitchStateLogic() {
            if (Cc.GroundFlag)
                StateMachine.ChangeState(StateMachine.LandingStateDriver);
        }
        
        public override void ExitStateLogic() {
            Cc.SetSensorRange(ControllerHelper.RaycastLength.Normal);
        }
    }
}