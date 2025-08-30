using Unity.Cinemachine;
using UnityEngine;
using SpellBound.Controller.ManagersAndStatics;
using SpellBound.Controller.PlayerInputs;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Interface for how the player inputs drive a camera from the rig.
    /// </summary>
    public class CameraController : MonoBehaviour {
        // What do we want the camera to pivot around?
        [SerializeField] private Transform cameraPivot;
        // How high you can move your camera.
        [SerializeField, Range(0f, 90f)] private float upperVerticalLimit = 89f;
        // How low you can move your camera.
        [SerializeField, Range(0f, 90f)] private float lowerVerticalLimit = 89f;
        // How fast you can move your camera.
        [SerializeField] private float cameraSpeed = 0.5f;
        // Optional lerp applied to input.
        [SerializeField] private bool smoothCameraRotation;
        // Multiplies the time.deltaTime by this factor.
        [SerializeField, Range(1f, 50f)] private float cameraSmoothingFactor = 25f;
        [SerializeField, Min(0.1f)] private float minZoomDistance = 1f;
        [SerializeField, Min(1f)] private float maxZoomDistance = 8f;
        // Controls how fast you zoom in and out.
        [SerializeField] private float zoomIncrement = .2f;
        [SerializeField] private bool cursorLockOnStart = true;
        [SerializeField] private PlayerInputActionsSO input;
        [SerializeField] private Helper.CameraCouplingMode playerRotationMode;
        [SerializeField] private bool cameraFollowMouse = true;
        
        private CameraRigManager _cameraRig;
        private CinemachineBrain _brain;
        private Transform _tr;
        // Cached local rotation value X.
        private float _currentXAngle;
        // Cached local rotation value Y.
        private float _currentYAngle;
        
        private void Awake() {
            _tr = transform;
        }

        private void OnEnable() {
            
        }

        private void OnDisable() {
            if (_cameraRig)
                input.OnMouseWheelInput -= ZoomCamera;
        }

        private void Start() {
            Cursor.lockState = cursorLockOnStart
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;

            if (input == null)
                input = InputManager.Instance.GetInputs();
            
            if (!_brain && Camera.main) 
                Camera.main.TryGetComponent(out _brain);
            
            if (!_brain)
                _brain = FindFirstObjectByType<CinemachineBrain>();
            
            if (!_brain)
                Debug.LogError("No brain found. CinemachineBrain missing from scene.", this);
            
            _currentXAngle = _tr.localRotation.eulerAngles.x;
            _currentYAngle = _tr.localRotation.eulerAngles.y;
            
            if (CameraRigManager.Instance == null)
                Debug.LogError("CameraController has a dependency on CameraRigManager. Please ensure the camera rig" +
                               "prefab is in the scene or the CameraRigManager script is on your custom camera rig.", 
                        this);
            
            if (SyncTransform.Instance == null)
                Debug.LogError("CameraController has a dependency on SyncTransform. Please ensure the CameraFollow" +
                               "prefab is in the scene or the SyncTransform script is on your custom object.", 
                        this);
            
            _cameraRig = CameraRigManager.Instance;
            
            if (input)
                input.OnMouseWheelInput += ZoomCamera;
            
            CameraSetup();
        }

        private void Update() {
            RotateCamera(input.LookDirection.x, -input.LookDirection.y);
        }

        public Vector3 GetUpDirection() => _tr.up;
        public Vector3 GetFacingDirection() => _tr.forward;
        
        /// <summary>
        /// Rotates the camera based on the device horizontal and vertical input about the pivot.
        /// </summary>
        private void RotateCamera(float horizontalInput, float verticalInput) {
            if (!cameraFollowMouse)
                return;
            
            var targetX = _currentXAngle + verticalInput  * cameraSpeed;
            var targetY = _currentYAngle + horizontalInput * cameraSpeed;

            targetX = Mathf.Clamp(targetX, -upperVerticalLimit, lowerVerticalLimit);

            if (smoothCameraRotation) {
                var blend = 1f - Mathf.Exp(-cameraSmoothingFactor * Time.unscaledDeltaTime);
                _currentXAngle = Mathf.LerpAngle(_currentXAngle, targetX, blend);
                _currentYAngle = Mathf.LerpAngle(_currentYAngle, targetY, blend);
            }
            else {
                _currentXAngle = targetX;
                _currentYAngle = targetY;
            }

            _currentXAngle = Mathf.Clamp(_currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
            cameraPivot.localRotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0f);
        }

        private void ZoomCamera(Vector2 zoomInput) {
            if (!cameraFollowMouse)
                return;
            
            var currentZoom = _cameraRig.GetCurrentCameraZoom();

            if (float.IsNaN(currentZoom))
                return;

            var target = currentZoom - zoomInput.y * zoomIncrement;
            target = Mathf.Clamp(target, minZoomDistance, maxZoomDistance);
            CameraRigManager.Instance.SetCameraZoom(target);
        }
        
        /// <summary>
        /// Sets our cameraRig tracking target.
        /// </summary>
        private void CameraSetup() {
            SyncTransform.Instance.SetFollowTransform(gameObject.transform);

            if (!cameraPivot)
                cameraPivot = SyncTransform.Instance.transform;
            
            _brain.WorldUpOverride = cameraPivot;

            if (_cameraRig == null) {
                Debug.LogError("Camera rig is null and doesn't appear to be in scene.");
                return;
            }

            _cameraRig.DefaultTarget.Target.TrackingTarget = cameraPivot;
        }
        
        public void SetCameraFollowMouse(bool follow) => cameraFollowMouse = follow;
    }
}