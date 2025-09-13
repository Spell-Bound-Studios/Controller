using System.Collections;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// I partitioned this state to be the in-between of the ground state before it and falling or landing in the event
    /// you wanted to play a sound or play an animation.
    /// </summary>
    [CreateAssetMenu(fileName = "JumpStateExample", menuName = "Spellbound/StateMachine/JumpStateExample")]
    public class JumpStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        protected float DefaultJumpMultiplier = 1.0f;
        
        private readonly WaitForSeconds _minJumpDuration = new(0.3f);
        private readonly WaitForSeconds _maxJumpDuration = new(2f);
        private Coroutine _jumpMinRoutine;
        private Coroutine _jumpMaxRoutine;
        
        protected override void OnCtxInitialized() {
            Ctx = base.Ctx as PlayerControllerExample;
        }

        protected override void EnterStateLogic() {
            _jumpMinRoutine = Ctx.StartCoroutine(JumpMinRoutine());
            _jumpMaxRoutine = Ctx.StartCoroutine(JumpMaxRoutine());
            
            Jump();
        }
        protected override void UpdateStateLogic() {
            
        }
        protected override void FixedUpdateStateLogic() {
            
        }
        protected override void ExitStateLogic() {
            
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