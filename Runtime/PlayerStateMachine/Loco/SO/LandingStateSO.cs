using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "LandingState", menuName = "Spellbound/LocoStates/LandingState")]
    public class LandingStateSO : BaseLocoStateSO {
        // Landing Thresholds
        private readonly WaitForSeconds _landingDuration = new(0.25f);
        private Coroutine _landRoutine;
        
        public override void EnterStateLogic(LocoStateMachine stateMachine) {
            StateMachine = stateMachine;
            StateHelper.NotifyLocoStateChange(this);
            StateHelper.NotifyLocoAnimationChange(StateHelper.States.Landing);
            
            // Coroutines must yield before state checks begin.
            _landRoutine = StateMachine.PlayerController.StartCoroutine(LandRoutine());
        }
        
        public override void UpdateStateLogic(in LocoStateContext ctx) {
            CheckSwitchStateLogic(in ctx);
        }
        
        public override void FixedUpdateStateLogic(in LocoStateContext ctx) {

        }
        
        public override void CheckSwitchStateLogic(in LocoStateContext ctx) {
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
    }
}