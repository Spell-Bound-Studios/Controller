// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    public class CameraRigManager : CinemachineCameraManagerBase, ICameraRig {
        [SerializeField,
         Tooltip("Camera made live on start (by name). Falls back to the first entry in Cameras when empty/unmatched.")]
        private string defaultCamera;

        [SerializeField, Tooltip("The rig's cameras; switched between by GameObject name.")]
        private List<CinemachineCamera> cameras = new();

        [Header("Lens")]
        [SerializeField, Tooltip("Near clip plane applied to every rig camera, so switching cameras never reverts it.")]
        private float nearClipPlane = 0.3f;

        [SerializeField, Tooltip("Far clip plane applied to every rig camera, so switching cameras never reverts it. " +
                                 "Raise it for long view distances (distant terrain, ocean planes).")]
        private float farClipPlane = 1000f;

        private readonly Dictionary<string, CinemachineCamera> _byName = new();
        private readonly List<string> _names = new();

        private CinemachineCamera _currentCamera;
        private string _currentName;

        public string Current => _currentName;

        public Transform CurrentCameraTransform => _currentCamera != null
                ? _currentCamera.transform
                : null;

        public IReadOnlyList<string> CameraNames => _names;

        public event Action<string, string> CurrentChanged;

        public float NearClipPlane {
            get => nearClipPlane;
            set {
                nearClipPlane = value;
                ApplyClipPlanes();
            }
        }

        public float FarClipPlane {
            get => farClipPlane;
            set {
                farClipPlane = value;
                ApplyClipPlanes();
            }
        }

        private void Awake() {
            if (SingletonManager.TryGetSingletonInstance<ICameraRig>(out var existing) && !ReferenceEquals(existing, this)) {
                Destroy(gameObject);

                return;
            }

            SingletonManager.RegisterSingleton<ICameraRig>(this);
            BuildIndex();
            ApplyClipPlanes();
        }

        protected override void OnDestroy() {
            if (SingletonManager.TryGetSingletonInstance<ICameraRig>(out var registered) && ReferenceEquals(registered, this))
                SingletonManager.UnregisterSingleton<ICameraRig>();

            base.OnDestroy();
        }

        protected override void Start() {
            base.Start();

            var initial = ResolveDefaultName();

            if (initial != null)
                Switch(initial);
            else
                Log.Error("[CameraRigManager] No cameras configured.");
        }

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

        public bool Register(CinemachineCamera camera) {
            if (camera == null) {
                Log.Error("[CameraRigManager] Cannot register a null camera.");

                return false;
            }

            if (!_byName.TryAdd(camera.name, camera)) {
                Log.Error($"[CameraRigManager] Duplicate camera name '{camera.name}'.");

                return false;
            }

            if (camera.transform.parent != transform)
                camera.transform.SetParent(transform, true);

            cameras.Add(camera);
            _names.Add(camera.name);
            ApplyClipPlanes();

            return true;
        }

        public bool Unregister(string cameraName) {
            if (string.IsNullOrEmpty(cameraName) || !_byName.TryGetValue(cameraName, out var camera))
                return false;

            _byName.Remove(cameraName);
            _names.Remove(cameraName);
            cameras.Remove(camera);

            if (_currentCamera != camera)
                return true;

            _currentCamera = null;
            _currentName = null;

            var fallback = ResolveDefaultName();

            if (fallback != null)
                Switch(fallback);
            else
                Log.Warn("[CameraRigManager] Unregistered the last camera; nothing is live.");

            return true;
        }

        public CinemachineCamera GetCurrentCamera() => _currentCamera;

        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) =>
                _currentCamera;

        private void ApplyClipPlanes() {
            nearClipPlane = Mathf.Max(nearClipPlane, 0.01f);
            farClipPlane = Mathf.Max(farClipPlane, nearClipPlane + 0.001f);

            foreach (var camera in cameras) {
                if (camera == null)
                    continue;

                camera.Lens.NearClipPlane = nearClipPlane;
                camera.Lens.FarClipPlane = farClipPlane;
            }
        }

        private void BuildIndex() {
            _byName.Clear();
            _names.Clear();

            foreach (var camera in cameras) {
                if (camera == null) {
                    Log.Error("[CameraRigManager] A camera entry is unassigned.");

                    continue;
                }

                if (!_byName.TryAdd(camera.name, camera)) {
                    Log.Error($"[CameraRigManager] Duplicate camera name '{camera.name}'.");

                    continue;
                }

                _names.Add(camera.name);
            }
        }

        private string ResolveDefaultName() =>
                !string.IsNullOrEmpty(defaultCamera) && _byName.ContainsKey(defaultCamera)
                        ? defaultCamera
                        : _names.Count > 0
                                ? _names[0]
                                : null;
    }
}
