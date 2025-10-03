using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// I partitioned this state to be the in-between of the state before it and grounding in the event you wanted to
    /// play a ground sound or a ground animation or delay the player from being able to jump immediately.
    /// </summary>
    [CreateAssetMenu(fileName = "LandingStateExample", menuName = "Spellbound/StateMachine/LandingStateExample")]
    public class LandingStateExample : BaseLocomotionStateExample {
        // Landing Thresholds
        private readonly WaitForSeconds _landingDuration = new(0.15f);
        private Coroutine _landRoutine;
        
        protected override void EnterStateLogic() {
            // Coroutines must yield before state checks begin.
            _landRoutine = Ctx.StartCoroutine(LandRoutine());
        }
        
        protected override void UpdateStateLogic() {
            if (_landRoutine == null) 
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Grounded);
        }
        
        protected override void FixedUpdateStateLogic() {
            if (PerformGroundCheck())
                KeepCapsuleFloating();
            HandleInput();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() {
            
        }
        
        private IEnumerator LandRoutine() {
            // Prevents immediately being able to jump again.
            yield return _landingDuration;
            _landRoutine = null;
        }
    }
}