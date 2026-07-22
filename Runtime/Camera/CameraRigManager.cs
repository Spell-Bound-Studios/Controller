// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// The central store for camera operations: holds the rig's Cinemachine cameras and blends to the live one,
    /// switched by name. This is the only camera MonoBehaviour — Cinemachine's
    /// <see cref="CinemachineCameraManagerBase"/> must live on the rig GameObject. Drive it via <see cref="ICameraRig"/>.
    /// </summary>
    public class CameraRigManager : CinemachineCameraManagerBase, ICameraRig {
        [SerializeField,
         Tooltip("Camera made live on start (by name). Falls back to the first entry in Cameras when empty/unmatched.")]
        private string defaultCamera;

        [SerializeField, Tooltip("The rig's cameras; switched between by GameObject name.")]
        private List<CinemachineCamera> cameras = new();

        private readonly Dictionary<string, CinemachineCamera> _byName = new();

        private CinemachineCamera _currentCamera;
        private string _currentName;

        public string Current => _currentName;

        public Transform CurrentCameraTransform => _currentCamera != null
                ? _currentCamera.transform
                : null;

        /// <summary>
        /// The names of every camera on the rig, in declaration order.
        /// </summary>
        public IReadOnlyList<string> CameraNames { get; private set; } = new List<string>();

        public event Action<string, string> CurrentChanged;

        private void Awake() {
            if (SingletonManager.TryGetSingletonInstance<ICameraRig>(out var existing) && !ReferenceEquals(existing, this)) {
                Destroy(gameObject);

                return;
            }

            SingletonManager.RegisterSingleton<ICameraRig>(this);
            BuildIndex();
        }

        protected override void OnDestroy() {
            if (SingletonManager.TryGetSingletonInstance<ICameraRig>(out var registered) && ReferenceEquals(registered, this))
                SingletonManager.UnregisterSingleton<ICameraRig>();

            base.OnDestroy();
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

        public string Switch(string cameraName) {
            var previous = Current;

            if (string.IsNullOrEmpty(cameraName) || !_byName.TryGetValue(cameraName, out var camera)) {
                Log.Warn($"[CameraRigManager] No camera named '{cameraName}'.");

                return previous;
            }

            if (camera == _currentCamera)
                return previous;

            _currentCamera = camera;
            _currentName = cameraName;
            CurrentChanged?.Invoke(previous, cameraName);

            return previous;
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
