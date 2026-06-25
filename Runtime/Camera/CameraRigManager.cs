// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using Spellbound.Core.Logging;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// The central store for camera operations: holds the rig's Cinemachine cameras and blends to the live one,
    /// switched by name. This is the only camera MonoBehaviour — Cinemachine's
    /// <see cref="CinemachineCameraManagerBase"/> must live on the rig GameObject. Drive it via <see cref="ICameraRig"/>.
    /// </summary>
    public class CameraRigManager : CinemachineCameraManagerBase, ICameraRig {
        public static CameraRigManager Instance;

        [SerializeField,
         Tooltip("Camera made live on start (by name). Falls back to the first entry in Cameras when empty/unmatched.")]
        private string defaultCamera;

        [SerializeField, Tooltip("The rig's cameras; switched between by GameObject name.")]
        private List<CinemachineCamera> cameras = new();

        private readonly Dictionary<string, CinemachineCamera> _byName = new();

        private CinemachineCamera _currentCamera;

        public string Current => _currentCamera != null
                ? _currentCamera.name
                : null;

        public Transform CurrentCameraTransform => _currentCamera != null
                ? _currentCamera.transform
                : null;

        /// <summary>
        /// The names of every camera on the rig, in declaration order.
        /// </summary>
        public IReadOnlyList<string> CameraNames { get; private set; } = new List<string>();

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;
            BuildIndex();
        }

        protected override void Start() {
            base.Start();

            var initial = !string.IsNullOrEmpty(defaultCamera) && _byName.ContainsKey(defaultCamera)
                    ? defaultCamera
                    : CameraNames.Count > 0
                            ? CameraNames[0]
                            : null;

            if (initial != null)
                Switch(initial);
            else
                Log.Error("[CameraRigManager] No cameras configured.");
        }

        /// <summary>
        /// Points the rig's default target at the consumer's runtime pivot; every child camera tracks it.
        /// </summary>
        public void SetFollowTarget(Transform target) => DefaultTarget.Target.TrackingTarget = target;

        public void Switch(string cameraName) {
            if (string.IsNullOrEmpty(cameraName) || !_byName.TryGetValue(cameraName, out var camera)) {
                Log.Warn($"[CameraRigManager] No camera named '{cameraName}'.");

                return;
            }

            _currentCamera = camera;
        }

        /// <summary>
        /// The live Cinemachine camera (e.g. for world-space UI / billboards that need it directly).
        /// </summary>
        public CinemachineCamera GetCurrentCamera() => _currentCamera;

        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) =>
                _currentCamera;

        private void BuildIndex() {
            var names = new List<string>();

            foreach (var camera in cameras) {
                if (camera == null) {
                    Log.Error("[CameraRigManager] A camera entry is unassigned.");

                    continue;
                }

                if (!_byName.TryAdd(camera.name, camera)) {
                    Log.Error($"[CameraRigManager] Duplicate camera name '{camera.name}'.");

                    continue;
                }

                names.Add(camera.name);
            }

            CameraNames = names;
        }
    }
}
