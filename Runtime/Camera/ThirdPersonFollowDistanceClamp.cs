// Copyright 2026 Spellbound Studio Inc.

using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    [RequireComponent(typeof(CinemachineThirdPersonFollow))]
    public class ThirdPersonFollowDistanceClamp : CinemachineExtension {
        [SerializeField, Range(0f, 2f),
         Tooltip("Closest the camera may get to the follow target, so obstacle pull-in never puts it inside the " +
                 "character. Higher = stops further out; 0 = no floor.")]
        private float minDistance = 0.75f;

        [SerializeField, Range(0f, 10f),
         Tooltip("Furthest the damped camera may trail beyond its authored camera distance before being dragged " +
                 "along, so fast movement cannot leave the character offscreen. Lower = tighter follow; higher = " +
                 "looser trailing.")]
        private float maxLag = 1f;

        private CinemachineThirdPersonFollow _follow;

        public float MinDistance {
            get => minDistance;
            set => minDistance = value;
        }

        public float MaxLag {
            get => maxLag;
            set => maxLag = value;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            if (stage != CinemachineCore.Stage.Body)
                return;

            if (_follow == null && !TryGetComponent(out _follow))
                return;

            if (_follow.FollowTarget == null)
                return;

            var targetPosition = _follow.FollowTargetPosition;
            var toCamera = state.RawPosition - targetPosition;
            var distance = toCamera.magnitude;
            var maxDistance = Mathf.Max(minDistance, _follow.CameraDistance + maxLag);

            if (distance < 1e-4f) {
                state.RawPosition = targetPosition + state.RawOrientation * (Vector3.back * minDistance);

                return;
            }

            var clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);

            if (!Mathf.Approximately(clampedDistance, distance))
                state.RawPosition = targetPosition + toCamera * (clampedDistance / distance);
        }
    }
}
