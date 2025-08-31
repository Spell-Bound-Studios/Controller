using SpellBound.Controller.PlayerController;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    public class CharacterControllerExample : SbCharacterControllerBase {
        [SerializeField] private Animator animator;
        
        protected override AnimationControllerBase CreateAnimationController() {
            if (animator == null) {
                animator = GetComponentInChildren<Animator>();
                if (animator == null) {
                    Debug.LogError("PurrNetCharController: NetworkAnimator not found.");
                    return null;
                }
            }

            return new AnimationControllerExample(animator);
        }
    }
}