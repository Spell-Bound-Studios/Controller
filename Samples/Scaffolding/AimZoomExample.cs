// Copyright 2026 Spellbound Studio Inc.

using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [RequireComponent(typeof(CinemachineCamera), typeof(CinemachineOrbitalFollow))]
    public sealed class AimZoomExample : MonoBehaviour {
        [SerializeField, Range(0.1f, 1f),
         Tooltip("Orbit scale eased to while aiming. Lower = closer over the shoulder.")]
        private float aimOrbitScale = 0.6f;

        [SerializeField, Range(20f, 80f),
         Tooltip("Lens field of view eased to while aiming. Lower = stronger zoom.")]
        private float aimFieldOfView = 50f;

        [SerializeField, Range(1f, 30f),
         Tooltip("How quickly the zoom eases in and back out. Higher = snappier.")]
        private float zoomDamping = 8f;

        private CinemachineCamera _camera;
        private CinemachineOrbitalFollow _orbit;
        private float _restOrbitScale;
        private float _restFieldOfView;
        private bool _aiming;
        private bool _easing;

        public bool Aiming {
            get => _aiming;
            set {
                if (_aiming == value)
                    return;

                _aiming = value;

                if (value) {
                    _restOrbitScale = _orbit.RadialAxis.Value;
                    _restFieldOfView = _camera.Lens.FieldOfView;
                }

                _easing = true;
            }
        }

        private void Awake() {
            _camera = GetComponent<CinemachineCamera>();
            _orbit = GetComponent<CinemachineOrbitalFollow>();
        }

        private void Update() {
            if (!_easing)
                return;

            var targetScale = _aiming
                    ? aimOrbitScale
                    : _restOrbitScale;
            var targetFieldOfView = _aiming
                    ? aimFieldOfView
                    : _restFieldOfView;
            var blend = 1f - Mathf.Exp(-zoomDamping * Time.deltaTime);

            _orbit.RadialAxis.Value = Mathf.Lerp(_orbit.RadialAxis.Value, targetScale, blend);
            _camera.Lens.FieldOfView = Mathf.Lerp(_camera.Lens.FieldOfView, targetFieldOfView, blend);

            if (Mathf.Abs(_orbit.RadialAxis.Value - targetScale) > 0.001f ||
                Mathf.Abs(_camera.Lens.FieldOfView - targetFieldOfView) > 0.01f)
                return;

            _orbit.RadialAxis.Value = targetScale;
            _camera.Lens.FieldOfView = targetFieldOfView;
            _easing = false;
        }
    }
}
