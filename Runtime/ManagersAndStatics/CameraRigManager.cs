using System.Collections.Generic;
using SpellBound.Controller.PlayerController;
using Unity.Cinemachine;
using UnityEngine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.ManagersAndStatics {
    public class CameraRigManager : CinemachineCameraManagerBase {
        public static CameraRigManager Instance;
        
        private readonly Dictionary<Helper.CameraType, CinemachineCamera> _cinemachineCameras = new();
        private readonly Dictionary<Helper.CameraType, CinemachineThirdPersonFollow> _thirdPersonCameras = new();
        private Helper.CameraType _currentType = Helper.CameraType.Default;
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

            if (_cinemachineCameras.TryGetValue(Helper.CameraType.Default, out var defaultCam)) {
                _currentType = Helper.CameraType.Default;
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
                Debug.LogWarning($"[CameraRigManager] No Default camera found. Falling back to {_currentCinemachineCamera.name}.");
            }
            else {
                Debug.LogError("[CameraRigManager] No cameras found at all!", this);
            }
        }

        protected override void Update() {
            base.Update();
            
            if (Input.GetKeyDown(KeyCode.E)) {
                if (_currentType == Helper.CameraType.Default && _cinemachineCameras.ContainsKey(Helper.CameraType.Zoomed)) {
                    SwitchCamera(Helper.CameraType.Zoomed);
                }
                else if (_cinemachineCameras.ContainsKey(Helper.CameraType.Default)) {
                    SwitchCamera(Helper.CameraType.Default);
                }
            }
        }
        
        public CinemachineCamera GetCurrentCamera() => _currentCinemachineCamera;
        public float GetCurrentCameraZoom() =>
                _currentThirdPersonCamera != null
                        ? _currentThirdPersonCamera.CameraDistance
                        : float.NaN;
        
        public void SwitchCamera(Helper.CameraType cameraType) {
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
        
        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) {
            return _currentCinemachineCamera;
        }
    }
}