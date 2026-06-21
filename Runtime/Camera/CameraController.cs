// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Plain (non-MonoBehaviour) camera control API. The consumer instantiates it and calls the capabilities it
    /// wants from wherever it likes — e.g. <see cref="TrackTarget"/> and <see cref="ApplyLook"/> from a LateUpdate,
    /// <see cref="Zoom"/> / <see cref="SwitchCamera"/> from input events. Input is fed in (never read here) so any
    /// binding drives it. Cinemachine owns the actual camera positioning and its update timing; this only drives
    /// the pivot the cameras track, switches the live camera, and adjusts zoom.
    /// </summary>
    public sealed class CameraController {
        private readonly ICameraRig _rig;
        private readonly Transform _pivot;
        private readonly Transform _followTarget;
        private readonly ICameraSettings _settings;

        private Vector3 _offset;
        private float _yaw;
        private float _pitch;
        private float _smoothedYaw;
        private float _smoothedPitch;

        public CameraController(ICameraRig rig, Transform pivot, Transform followTarget, ICameraSettings settings,
            Vector3 offset = default) {
            _rig = rig;
            _pivot = pivot;
            _followTarget = followTarget;
            _settings = settings;
            _offset = offset;

            var euler = pivot != null
                    ? pivot.localRotation.eulerAngles
                    : Vector3.zero;
            _yaw = _smoothedYaw = euler.y;
            _pitch = _smoothedPitch = euler.x;
        }

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
        /// Adjusts the live camera's zoom by <paramref name="delta"/>, clamped to the current profile's range.
        /// </summary>
        public void Zoom(float delta) {
            if (_rig == null || float.IsNaN(_rig.Zoom))
                return;

            var range = _rig.Current != null
                    ? _rig.Current.ZoomRange
                    : new Vector2(float.NegativeInfinity, float.PositiveInfinity);

            _rig.Zoom = Mathf.Clamp(_rig.Zoom + delta, range.x, range.y);
        }

        /// <summary>
        /// Switches the live camera to <paramref name="profile"/>; the rig blends to it.
        /// </summary>
        public void SwitchCamera(CameraProfile profile) => _rig?.Switch(profile);
    }
}
