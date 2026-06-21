// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// The central store for camera operations: maps each <see cref="CameraProfile"/> to a Cinemachine camera and
    /// blends to the live one. This is the only camera MonoBehaviour — Cinemachine's
    /// <see cref="CinemachineCameraManagerBase"/> must live on the rig GameObject. Drive it via <see cref="ICameraRig"/>.
    /// </summary>
    public class CameraRigManager : CinemachineCameraManagerBase, ICameraRig {
        public static CameraRigManager Instance;

        [SerializeField] private Transform pivot;
        [SerializeField] private CameraProfile defaultProfile;
        [SerializeField] private List<CameraBinding> cameras = new();

        private readonly Dictionary<CameraProfile, CinemachineCamera> _byProfile = new();
        private readonly Dictionary<CameraProfile, CinemachineThirdPersonFollow> _zoomers = new();

        private CinemachineCamera _currentCamera;
        private CinemachineThirdPersonFollow _currentZoomer;

        /// <summary>
        /// The transform the live camera tracks — the consumer drives this (follow + look).
        /// </summary>
        public Transform Pivot => pivot;
        
        public CameraProfile Current { get; private set; }

        public Transform CurrentCameraTransform => _currentCamera != null
                ? _currentCamera.transform
                : null;

        public float Zoom {
            get => _currentZoomer != null
                    ? _currentZoomer.CameraDistance
                    : float.NaN;
            set {
                if (_currentZoomer != null)
                    _currentZoomer.CameraDistance = value;
            }
        }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;
        }

        protected override void Start() {
            base.Start();

            foreach (var binding in cameras) {
                if (binding?.Profile == null || binding.Camera == null) {
                    Log.Error("[CameraRigManager] A camera binding is missing its profile or camera.");

                    continue;
                }

                if (!_byProfile.TryAdd(binding.Profile, binding.Camera)) {
                    Log.Error(
                        $"[CameraRigManager] Duplicate camera binding for profile '{binding.Profile.name}'.");

                    continue;
                }

                _zoomers[binding.Profile] = binding.Camera.GetComponent<CinemachineThirdPersonFollow>();
            }

            if (pivot != null)
                DefaultTarget.Target.TrackingTarget = pivot;

            var initial = defaultProfile != null && _byProfile.ContainsKey(defaultProfile)
                    ? defaultProfile
                    : cameras.Count > 0
                            ? cameras[0]?.Profile
                            : null;

            if (initial != null)
                Switch(initial);
            else
                Log.Error("[CameraRigManager] No camera bindings configured.");
        }

        public void Switch(CameraProfile profile) {
            if (profile == null || !_byProfile.TryGetValue(profile, out var camera)) {
                Log.Warn(
                    $"[CameraRigManager] No camera bound for profile '{(profile != null ? profile.name : "null")}'.");

                return;
            }

            Current = profile;
            _currentCamera = camera;
            _zoomers.TryGetValue(profile, out _currentZoomer);
        }

        /// <summary>
        /// The live Cinemachine camera (e.g. for world-space UI / billboards that need it directly).
        /// </summary>
        public CinemachineCamera GetCurrentCamera() => _currentCamera;

        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) =>
                _currentCamera;

        [Serializable]
        private class CameraBinding {
            [SerializeField] private CameraProfile profile;
            [SerializeField] private CinemachineCamera camera;
            public CameraProfile Profile => profile;
            public CinemachineCamera Camera => camera;
        }
    }
}
