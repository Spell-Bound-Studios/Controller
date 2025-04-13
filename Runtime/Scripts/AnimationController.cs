using SpellBound.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller {
    public class AnimationController {
        private readonly StateContext _ctx;
        private readonly Animator _animator;
        private readonly Rigidbody _rigidbody;
        
        private static readonly int Speed = Animator.StringToHash("speed");
        
        public AnimationController(Animator animator, StateContext ctx) {
            _animator = animator;
            _ctx = ctx;
            
            _ctx.OnStateChanged += HandleStateChange;
            _ctx.OnAnimationSpeedChanged += HandleAnimationSpeedChanged;
        }

        public void DisposeEvents() {
            _ctx.OnStateChanged -= HandleStateChange;
            _ctx.OnAnimationSpeedChanged += HandleAnimationSpeedChanged;
        }

        private void HandleStateChange(AnimationStates.States state) {
            // Default into standing?
            if (AnimationStates.StateHashes.TryGetValue(state, out var hash)) {
                _animator.Play(hash);
            }
            else {
                Debug.LogWarning($"Animation hash not found in dictionary: {state}.");
            }
        }

        private void HandleAnimationSpeedChanged(float speed) {
            _animator.SetFloat(Speed, speed);
            Debug.Log($"Speed is : {speed}");
        }
    }
}