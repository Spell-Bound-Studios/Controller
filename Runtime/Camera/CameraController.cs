// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    public sealed class CameraController : IDisposable {
        private readonly ICameraRig _rig;
        private readonly Transform _pivot;
        private readonly Transform _followTarget;
        private readonly ICameraSettings _settings;
        private readonly bool _ownsPivot;

        private Vector3 _offset;
        private float _yaw;
        private float _pitch;
        private float _smoothedYaw;
        private float _smoothedPitch;

        public CameraController(ICameraRig rig, Transform followTarget, ICameraSettings settings,
            Vector3 offset = default)
            : this(rig, followTarget, settings, new GameObject("CameraPivot").transform, offset) =>
                _ownsPivot = true;

        public CameraController(ICameraRig rig, Transform followTarget, ICameraSettings settings, Transform pivot,
            Vector3 offset = default) {
            _rig = rig;
            _followTarget = followTarget;
            _settings = settings;
            _pivot = pivot;
            _offset = offset;

            _yaw = _smoothedYaw = followTarget != null
                    ? followTarget.eulerAngles.y
                    : 0f;
            _pitch = _smoothedPitch = 0f;

            TrackTarget();

            if (_pivot != null)
                _pivot.rotation = Quaternion.Euler(_pitch, _yaw, 0f);

            _rig?.SetFollowTarget(_pivot);
        }

        public Transform Pivot => _pivot;

        public Vector3 Offset {
            get => _offset;
            set => _offset = value;
        }

        public void TrackTarget() {
            if (_pivot == null || _followTarget == null)
                return;

            _pivot.position = _followTarget.position + _offset;
        }

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

            _pivot.rotation = Quaternion.Euler(_smoothedPitch, _smoothedYaw, 0f);
        }

        public string SwitchCamera(string cameraName) => _rig?.Switch(cameraName);

        public void Dispose() {
            if (_ownsPivot && _pivot != null)
                UnityEngine.Object.Destroy(_pivot.gameObject);
        }
    }
}
