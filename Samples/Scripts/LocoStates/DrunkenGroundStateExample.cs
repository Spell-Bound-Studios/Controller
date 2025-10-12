// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "DrunkenGroundStateExample",
        menuName = "Spellbound/StateMachine/DrunkenGroundStateExample")]
    public class DrunkenGroundStateExample : GroundStateExample {
        protected override Vector3 GetInputDirectionRelativeToCamera() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                        Ctx.referenceTransform.right, Ctx.planarUp).normalized *
                    Ctx.input.Direction.y +
                    Vector3.ProjectOnPlane(
                        Ctx.referenceTransform.forward, Ctx.planarUp).normalized *
                    Ctx.input.Direction.x;

            return direction.magnitude > 1f
                    ? direction.normalized
                    : direction;
        }

        protected override void HandleInteractPressed() =>
                Ctx.locoStateMachine.ChangeVariant(LocoStateTypes.Grounded, Ctx.locoStates[0]);
    }
}