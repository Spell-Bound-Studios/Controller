using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "JumpingState", menuName = "Spellbound/LocoStates/JumpingState")]
    public class JumpingStateSO : BaseLocoStateSO {
        private readonly WaitForSeconds _minJumpDuration = new(0.3f);
        private readonly WaitForSeconds _maxJumpDuration = new(3f);
        private Coroutine _jumpMinRoutine;
        private Coroutine _jumpMaxRoutine;
        
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Jumping);
            
            _jumpMinRoutine = Cc.StartCoroutine(JumpMinRoutine());
            _jumpMaxRoutine = Cc.StartCoroutine(JumpMaxRoutine());

            Cc.Jump();
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            Cc.HandleHorizontalVelocityInput();
        }
        
        public override void CheckSwitchStateLogic() {
            // If the minimum time expires
            if (_jumpMinRoutine == null) {
                // Check grounded and exit ground state.
                if (Cc.GroundFlag)
                    StateMachine.ChangeState(StateMachine.LandingStateDriver);
            }
            // If maximum time expires
            if (_jumpMaxRoutine == null) {
                StateMachine.ChangeState(StateMachine.FallingStateDriver);
            }
        }
        
        public override void ExitStateLogic() {
            if (_jumpMaxRoutine != null)
                StateMachine.CharController.StopCoroutine(_jumpMaxRoutine);

            Cc.jumpFlag = false;
        }
        
        private IEnumerator JumpMinRoutine() {
            // Prevents immediate transition back to GroundedState.
            yield return _minJumpDuration;
            _jumpMinRoutine = null;
        }
        
        private IEnumerator JumpMaxRoutine() {
            yield return _maxJumpDuration;
            _jumpMaxRoutine = null;
        }
    }
}