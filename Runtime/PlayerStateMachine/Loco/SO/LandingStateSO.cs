using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "LandingState", menuName = "Spellbound/LocoStates/LandingState")]
    public class LandingStateSO : BaseLocoStateSO {
        // Landing Thresholds
        private readonly WaitForSeconds _landingDuration = new(0.15f);
        private Coroutine _landRoutine;
        
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Landing);
            
            // Coroutines must yield before state checks begin.
            _landRoutine = StateMachine.CharController.StartCoroutine(LandRoutine());
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            GroundCheck();
            HandleInput();
            HandleCharacterRotation();
        }
        
        public override void CheckSwitchStateLogic() {
            if (_landRoutine == null) 
                StateMachine.ChangeState(StateMachine.GroundStateDriver);
        }
        
        public override void ExitStateLogic() {
            
        }
        
        private IEnumerator LandRoutine() {
            // Prevents immediately being able to jump again.
            yield return _landingDuration;
            _landRoutine = null;
        }

        protected override void HandleAnimation() { }
    }
}