using SpellBound.Controller.PlayerController;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    public class AnimationControllerExample : AnimationControllerBase {
        private readonly Animator _animator;
    
        public AnimationControllerExample(Animator animator) {
            _animator = animator;
        }

        protected override void PlayAnimation(int stateHash) {
            _animator.Play(stateHash);
        }
        
        protected override void SetAnimationSpeed(int speed, float blendValue) {
            _animator.SetFloat(speed, blendValue);
        }
    }
}