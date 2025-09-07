using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "SlidingState", menuName = "Spellbound/LocoStates/SlidingState")]
    public class SlidingStateSO : BaseLocoStateSO{
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Grounded);
            
            Cc.ResizableCapsuleCollider.SlopeData.ClearStepHeightPercentage();
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            GroundCheck();
            HandleInput();
            HandleAnimation();
            HandleCharacterRotation();
        }
        
        public override void CheckSwitchStateLogic() {
            if (!Cc.StateData.Grounded)
                StateMachine.ChangeState(StateMachine.FallingStateDriver);
            
            if (Cc.StatData.slopeSpeedModifier != 0)
                StateMachine.ChangeState(StateMachine.GroundStateDriver);
        }
        
        public override void ExitStateLogic() {
            Cc.ResizableCapsuleCollider.SlopeData.RevertStepHeightPercentage();
        }

        protected override void HandleAnimation() {
            StateHelper.NotifyLocoAnimationSpeedChange(GetHorizontalSpeed());
        }
    }
}