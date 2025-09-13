﻿using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// I partitioned this state to be the in-between of the ground state before it and falling or landing in the event
    /// you wanted to play a sound or play an animation. It is meant to show how easy it is to create lockouts or small
    /// states for animations to play.
    /// </summary>
    [CreateAssetMenu(fileName = "JumpStateExample", menuName = "Spellbound/StateMachine/JumpStateExample")]
    public class JumpStateExample : BaseLocoStateExample {
        protected float DefaultJumpMultiplier = 1.0f;
        
        private readonly WaitForSeconds _minJumpDuration = new(0.3f);
        private readonly WaitForSeconds _maxJumpDuration = new(2f);
        private Coroutine _jumpMinRoutine;
        private Coroutine _jumpMaxRoutine;

        protected override void EnterStateLogic() {
            _jumpMinRoutine = Ctx.StartCoroutine(JumpMinRoutine());
            _jumpMaxRoutine = Ctx.StartCoroutine(JumpMaxRoutine());
            
            Jump();
        }

        protected override void UpdateStateLogic() {
            // If the minimum time expires
            if (_jumpMinRoutine == null) {
                // Check grounded and exit ground state.
                if (Ctx.StateData.Grounded)
                    Ctx.locoStateMachine.ChangeState(LocoStateTypes.Landing);
            }
            
            // If maximum time expires
            if (_jumpMaxRoutine == null)
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Falling);
        }
        protected override void ExitStateLogic() {
            if (_jumpMaxRoutine != null)
                Ctx.StopCoroutine(_jumpMaxRoutine);
        }

        /// <summary>
        /// The actual jump method that adds a force of some kind.
        /// </summary>
        protected virtual void Jump() {
            Ctx.Rb.AddForce(Ctx.StatData.jumpForce * Ctx.StatData.JumpMultiplier * DefaultJumpMultiplier * Ctx.planarUp, 
                    Ctx.RigidbodyData.verticalForceMode);
        }
        
        /// <summary>
        /// Simple coroutine to manage the minimum duration that a player should be locked to this state.
        /// </summary>
        private IEnumerator JumpMinRoutine() {
            // Prevents immediate transition back to GroundedState.
            yield return _minJumpDuration;
            _jumpMinRoutine = null;
        }
        
        /// <summary>
        /// Simple coroutines to manage the maximum duration that a player should be locked to this state.
        /// </summary>
        private IEnumerator JumpMaxRoutine() {
            yield return _maxJumpDuration;
            _jumpMaxRoutine = null;
        }
    }
}