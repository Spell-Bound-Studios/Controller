// Copyright 2025 Spellbound Studio Inc.

using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Spellbound.Controller {
    public class CameraRigManager : CinemachineCameraManagerBase {
        public static CameraRigManager Instance;

        private readonly Dictionary<ControllerHelper.CameraType, CinemachineCamera> _cinemachineCameras = new();

        private readonly Dictionary<ControllerHelper.CameraType, CinemachineThirdPersonFollow> _thirdPersonCameras =
                new();

        private ControllerHelper.CameraType _currentType = ControllerHelper.CameraType.Default;
        private CinemachineCamera _currentCinemachineCamera;
        private CinemachineThirdPersonFollow _currentThirdPersonCamera;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;
        }

        protected override void Start() {
            base.Start();

            foreach (var cam in ChildCameras) {
                var camBehaviour = cam.GetComponent<CameraTypeBehaviour>();

                if (!camBehaviour) {
                    Debug.LogError("Found a camera without a camera type behaviour.", this);

                    continue;
                }

                if (!_cinemachineCameras.TryAdd(camBehaviour.CameraType, (CinemachineCamera)cam)) {
                    Debug.LogError("Camera type already exists in the virtual cameras dict", this);

                    continue;
                }

                var followCam = cam.GetComponent<CinemachineThirdPersonFollow>();

                if (!_thirdPersonCameras.TryAdd(camBehaviour.CameraType, followCam)) {
                    Debug.LogError("Camera type already exists in the third person camera dict", this);

                    continue;
                }
            }

            if (_cinemachineCameras.TryGetValue(ControllerHelper.CameraType.Default, out var defaultCam)) {
                _currentType = ControllerHelper.CameraType.Default;
                _currentCinemachineCamera = defaultCam;
                _thirdPersonCameras.TryGetValue(_currentType, out _currentThirdPersonCamera);
            }
            else if (ChildCameras.Count > 0) {
                _currentCinemachineCamera = (CinemachineCamera)ChildCameras[0];

                var fallbackBehaviour = _currentCinemachineCamera.GetComponent<CameraTypeBehaviour>();

                _currentType = fallbackBehaviour
                        ? fallbackBehaviour.CameraType
                        : _currentType;
                _thirdPersonCameras.TryGetValue(_currentType, out _currentThirdPersonCamera);

                Debug.LogWarning(
                    $"[CameraRigManager] No Default camera found. Falling back to {_currentCinemachineCamera.name}.");
            }
            else
                Debug.LogError("[CameraRigManager] No cameras found at all!", this);
        }

        public CinemachineCamera GetCurrentCamera() => _currentCinemachineCamera;

        public float GetCurrentCameraZoom() =>
                _currentThirdPersonCamera != null
                        ? _currentThirdPersonCamera.CameraDistance
                        : float.NaN;

        public void SwitchCamera(ControllerHelper.CameraType cameraType) {
            if (!_cinemachineCameras.TryGetValue(cameraType, out var cinemachineCamera)) {
                Debug.LogWarning($"[CameraRigManager] No camera registered for type '{cameraType}'.");

                return;
            }

            _currentType = cameraType;
            _currentCinemachineCamera = cinemachineCamera;
            _thirdPersonCameras.TryGetValue(_currentType, out _currentThirdPersonCamera);
        }

        public void SetCameraZoom(float zoomValue) {
            if (_currentThirdPersonCamera != null)
                _currentThirdPersonCamera.CameraDistance = zoomValue;
        }

        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) =>
                _currentCinemachineCamera;
    }
}