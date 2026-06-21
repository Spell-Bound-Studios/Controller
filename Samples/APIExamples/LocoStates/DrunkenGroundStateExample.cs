// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    [CreateAssetMenu(fileName = "DrunkenGroundStateExample",
        menuName = "Spellbound/StateMachine/DrunkenGroundStateExample")]
    public class DrunkenGroundStateExample : GroundStateExample {
        protected override Vector3 GetInputDirectionRelativeToCamera() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                        Ctx.ReferenceTransform.right, Ctx.PlanarUp).normalized *
                    Ctx.ExampleInput.Direction.y +
                    Vector3.ProjectOnPlane(
                        Ctx.ReferenceTransform.forward, Ctx.PlanarUp).normalized *
                    Ctx.ExampleInput.Direction.x;

            return direction.magnitude > 1f
                    ? direction.normalized
                    : direction;
        }

        // Swap the slot back to its default (the base ground state).
        protected override void HandleInteractPressed() =>
                Ctx.LocoStateMachine.RestoreDefault(LocoStateTypes.Grounded);
    }
}
