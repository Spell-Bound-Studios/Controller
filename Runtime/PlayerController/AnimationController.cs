using PurrNet;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    public class AnimationController {
        private readonly NetworkAnimator _animator;
    
        private static readonly int Speed = Animator.StringToHash("speed");
    
        public AnimationController(NetworkAnimator animator) {
            _animator = animator;
            StateHelper.OnStateChanged += HandleStateChange;
            StateHelper.OnAnimationSpeedChanged += HandleAnimationSpeedChanged;
        }

        public void DisposeEvents() {
            StateHelper.OnStateChanged -= HandleStateChange;
            StateHelper.OnAnimationSpeedChanged -= HandleAnimationSpeedChanged;
        }

        private void HandleStateChange(StateHelper.States state) {
            // Default into standing?
            if (StateHelper.AnimationStateDict.TryGetValue(state, out var hash)) {
                _animator.Play(hash);
            }
            else {
                Debug.LogWarning($"Animation hash not found in dictionary: {state}.");
            }
        }

        private void HandleAnimationSpeedChanged(float blendValue) {
            _animator.SetFloat(Speed, blendValue);
        }
    }
    
}