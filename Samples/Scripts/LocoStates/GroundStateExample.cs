// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public class GroundStateExample : BaseLocomotionStateExample {
        [SerializeField] private BaseSoState drunkenVariant;

        protected override void EnterStateLogic() {
            Ctx.ExampleInput.OnInteractPressed += HandleInteractPressed;
            Ctx.ExampleInput.OnJumpPressed += HandleJumpPressed;
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.StateData.Grounded)
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Falling);
        }

        protected override void FixedUpdateStateLogic() {
            if (PerformGroundCheck())
                KeepCapsuleFloating();
            HandleInput();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() {
            Ctx.ExampleInput.OnInteractPressed -= HandleInteractPressed;
            Ctx.ExampleInput.OnJumpPressed -= HandleJumpPressed;
        }

        // Swap this slot to the drunken variant (e.g. as if a potion were consumed).
        protected virtual void HandleInteractPressed() =>
                Ctx.locoStateMachine.ApplyVariant(LocoStateTypes.Grounded, drunkenVariant);

        private void HandleJumpPressed() => Ctx.locoStateMachine.ChangeState(LocoStateTypes.Jumping);
    }
}
