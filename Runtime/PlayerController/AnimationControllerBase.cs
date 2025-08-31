using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    public abstract class AnimationControllerBase {
        protected static readonly int Speed = Animator.StringToHash("speed");

        protected AnimationControllerBase() {
            StateHelper.OnStateChanged += HandleStateChange;
            StateHelper.OnAnimationSpeedChanged += HandleAnimationSpeedChanged;
        }
        
        public void DisposeEvents() {
            StateHelper.OnStateChanged -= HandleStateChange;
            StateHelper.OnAnimationSpeedChanged -= HandleAnimationSpeedChanged;
        }
        
        private void HandleStateChange(StateHelper.States state) {
            // Default into standing?
            if (StateHelper.AnimationStateDict.TryGetValue(state, out var hash))
                PlayAnimation(hash);
            else
                Debug.LogWarning($"Animation hash not found in dictionary: {state}.");
        }
        
        private void HandleAnimationSpeedChanged(float blendValue) {
            SetAnimationSpeed(Speed, blendValue);
        }

        protected abstract void PlayAnimation(int stateHash);
        protected abstract void SetAnimationSpeed(int speed, float blendValue);
    }
}