// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    public abstract class BaseLocomotionStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        protected float HSpeedModifier = 1f;
        protected GroundProbeResult Ground = GroundProbeResult.Miss;

        protected override void OnCtxInitialized() => Ctx = base.Ctx as PlayerControllerExample;

        protected virtual void HandleInput() {
            var inputDir = GetInputDirectionRelativeToCamera();
            var moveDir = inputDir;
            var slopeSpeed = 1f;

            if (Ctx.StateData.Grounded) {
                moveDir = ControllerHelper.GetSlopeAdjustedDirection(inputDir, Ground.Normal, Ctx.planarUp);
                slopeSpeed = ResolveSlopeSpeed(inputDir);
            }

            var targetVelocity = slopeSpeed * HSpeedModifier * Ctx.StatData.movementSpeed * moveDir;
            var velocityChange = targetVelocity - ControllerHelper.GetHorizontalVelocity(Ctx.Rb);

            Ctx.Rb.AddForce(velocityChange, Ctx.RigidbodyData.horizontalForceMode);
        }

        /// <summary>
        /// Samples the slope-speed curve at the signed, normalized slope along the move direction
        /// (-1 uphill, +1 downhill).
        /// </summary>
        protected virtual float ResolveSlopeSpeed(Vector3 inputDir) {
            var curve = Ctx.StatData.slopeSpeedCurve;

            if (curve == null || curve.length == 0)
                return 1f;

            var up = Ctx.planarUp;
            var angle = ControllerHelper.GetSlopeAngle(Ground.Normal, up);
            var maxAngle = Ctx.StatData.maxSlopeAngle;

            if (angle < 0.5f || maxAngle < 0.01f || inputDir.sqrMagnitude < 1e-4f)
                return curve.Evaluate(0f);

            var alongSlope = Vector3.ProjectOnPlane(inputDir, Ground.Normal).normalized;
            var alignment = Vector3.Dot(alongSlope, ControllerHelper.GetSlopeDirection(Ground.Normal, up));
            var signedSlope = Mathf.Clamp(angle / maxAngle * alignment, -1f, 1f);

            return curve.Evaluate(signedSlope);
        }

        protected virtual Vector3 GetInputDirectionRelativeToCamera() =>
                ControllerHelper.GetInputDirectionRelativeToCamera(
                    Ctx.ExampleInput.Direction,
                    Ctx.referenceTransform,
                    Ctx.planarUp
                );

        /// <summary>
        /// Probes for ground beneath the capsule with a slope-scaled reach, caches the hit, and publishes
        /// <see cref="StateData.Grounded"/>.
        /// </summary>
        protected virtual bool PerformGroundCheck() {
            var floatData = Ctx.ResizableCapsuleCollider.CapsuleFloatData;
            var reach = floatData.RideHeight * SlopeReachFactor() + floatData.GroundedTolerance;

            Ground = Ctx.ResizableCapsuleCollider.ProbeGround(
                -Ctx.planarUp, reach + floatData.ProbeRadius, Ctx.LayerData.GroundLayer);

            Ctx.StateData.Grounded = Ground.HasHit && Ground.Distance <= reach;

            return Ctx.StateData.Grounded;
        }

        /// <summary>
        /// Applies the float-spring force that holds the capsule at its slope-scaled ride height.
        /// </summary>
        protected virtual void KeepCapsuleFloating() {
            if (!Ctx.StateData.Grounded)
                return;

            var floatData = Ctx.ResizableCapsuleCollider.CapsuleFloatData;

            var springForce = ControllerHelper.SolveFloatSpring(
                -Ctx.planarUp,
                Ground.Distance,
                floatData.RideHeight * SlopeReachFactor(),
                Ctx.Rb.linearVelocity,
                floatData.SpringStrength,
                floatData.SpringDamper);

            Ctx.Rb.AddForce(springForce, ForceMode.Acceleration);
        }

        /// <summary>
        /// The 1/cos factor that lengthens the straight-down ground reach so descents stay grounded.
        /// </summary>
        protected float SlopeReachFactor() =>
                ControllerHelper.GetSlopeReachFactor(
                    ControllerHelper.GetSlopeAngle(Ground.Normal, Ctx.planarUp),
                    Ctx.StatData.maxSlopeAngle);

        protected virtual void HandleCharacterRotation() =>
                ControllerHelper.HandleCharacterRotation(
                    Ctx.Rb,
                    Ctx.planarUp,
                    Ctx.RotationData.turnTowardsInputSpeed,
                    Ctx.RotationData.RotationFallOffAngle,
                    Time.fixedDeltaTime);
    }
}
