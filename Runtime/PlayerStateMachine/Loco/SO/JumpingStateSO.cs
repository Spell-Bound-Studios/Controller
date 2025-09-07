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

            Jump();
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            HandleInput();
            HandleCharacterRotation();
        }
        
        public override void CheckSwitchStateLogic() {
            // If the minimum time expires
            if (_jumpMinRoutine == null) {
                // Check grounded and exit ground state.
                if (Cc.StateData.Grounded)
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
        }
        
        protected override void HandleAnimation() { }
        
        private IEnumerator JumpMinRoutine() {
            // Prevents immediate transition back to GroundedState.
            yield return _minJumpDuration;
            _jumpMinRoutine = null;
        }
        
        private IEnumerator JumpMaxRoutine() {
            yield return _maxJumpDuration;
            _jumpMaxRoutine = null;
        }

        private void Jump() {
            Cc.Rb.AddForce(Cc.StatData.jumpForce * Cc.StatData.JumpMultiplier * Cc.planarUp, Cc.RigidbodyData.verticalForceMode);
        }
    }
}