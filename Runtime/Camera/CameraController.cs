// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Plain (non-MonoBehaviour) camera control API. The consumer instantiates it and calls the capabilities it
    /// wants from wherever it likes — e.g. <see cref="TrackTarget"/> and <see cref="ApplyLook"/> from a LateUpdate,
    /// <see cref="SwitchCamera"/> from input events. Input is fed in (never read here) so any binding drives it. It
    /// creates and owns a pivot the cameras track, drives that pivot (follow + look), and switches the live camera;
    /// Cinemachine owns the actual camera positioning and its update timing. <see cref="Dispose"/> destroys the pivot.
    /// </summary>
    public sealed class CameraController : IDisposable {
        private readonly ICameraRig _rig;
        private readonly Transform _pivot;
        private readonly Transform _followTarget;
        private readonly ICameraSettings _settings;

        private Vector3 _offset;
        private float _yaw;
        private float _pitch;
        private float _smoothedYaw;
        private float _smoothedPitch;

        public CameraController(ICameraRig rig, Transform followTarget, ICameraSettings settings,
            Vector3 offset = default) {
            _rig = rig;
            _followTarget = followTarget;
            _settings = settings;
            _offset = offset;

            _pivot = new GameObject("CameraPivot").transform;
            _yaw = _smoothedYaw = followTarget != null
                    ? followTarget.eulerAngles.y
                    : 0f;
            _pitch = _smoothedPitch = 0f;

            TrackTarget();
            _pivot.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);

            _rig?.SetFollowTarget(_pivot);
        }

        /// <summary>
        /// The runtime pivot this controller creates and drives; the rig tracks it. Exposed so the consumer can
        /// point Cinemachine's world-up override at it.
        /// </summary>
        public Transform Pivot => _pivot;

        /// <summary>
        /// The live camera's forward — the basis for camera-relative movement.
        /// </summary>
        public Vector3 ReferenceForward {
            get {
                var cam = _rig?.CurrentCameraTransform;

                return cam != null
                        ? cam.forward
                        : _pivot != null
                                ? _pivot.forward
                                : Vector3.forward;
            }
        }

        /// <summary>
        /// World-space offset added to the follow target when positioning the pivot.
        /// </summary>
        public Vector3 Offset {
            get => _offset;
            set => _offset = value;
        }

        /// <summary>
        /// Snaps the pivot onto the follow target (plus <see cref="Offset"/>). Call after movement resolves — e.g.
        /// from your LateUpdate — so the camera tracks the body without lag.
        /// </summary>
        public void TrackTarget() {
            if (_pivot == null || _followTarget == null)
                return;

            _pivot.position = _followTarget.position + _offset;
        }

        /// <summary>
        /// Turns the camera by a look delta: applies sensitivity, optional Y-invert, the pitch clamp, and optional
        /// smoothing, then writes the pivot's rotation. Feed it from any input source.
        /// </summary>
        public void ApplyLook(Vector2 delta) {
            if (_pivot == null || _settings == null)
                return;

            _yaw += delta.x * _settings.SensitivityX;
            _pitch += (_settings.InvertY
                    ? delta.y
                    : -delta.y) * _settings.SensitivityY;
            _pitch = Mathf.Clamp(_pitch, _settings.MinPitch, _settings.MaxPitch);

            if (_settings.SmoothLook) {
                var blend = 1f - Mathf.Exp(-_settings.SmoothingFactor * Time.unscaledDeltaTime);
                _smoothedYaw = Mathf.LerpAngle(_smoothedYaw, _yaw, blend);
                _smoothedPitch = Mathf.LerpAngle(_smoothedPitch, _pitch, blend);
            }
            else {
                _smoothedYaw = _yaw;
                _smoothedPitch = _pitch;
            }

            _pivot.localRotation = Quaternion.Euler(_smoothedPitch, _smoothedYaw, 0f);
        }

        /// <summary>
        /// Eases the camera yaw to line up behind the follow target's facing, at <paramref name="degreesPerSecond"/>.
        /// </summary>
        public void EaseBehind(float degreesPerSecond) {
            if (_followTarget == null)
                return;

            _yaw = Mathf.MoveTowardsAngle(_yaw, _followTarget.eulerAngles.y, degreesPerSecond * Time.deltaTime);
        }

        /// <summary>
        /// Switches the live camera to <paramref name="profile"/>; the rig blends to it.
        /// </summary>
        public void SwitchCamera(CameraProfile profile) => _rig?.Switch(profile);

        /// <summary>
        /// Destroys the runtime pivot this controller created. Call from the owner's OnDestroy.
        /// </summary>
        public void Dispose() {
            if (_pivot != null)
                UnityEngine.Object.Destroy(_pivot.gameObject);
        }
    }
}
