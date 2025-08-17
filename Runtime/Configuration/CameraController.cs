using Unity.Cinemachine;
using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    /// <summary>
    /// Interface for how the player inputs drive its members.
    /// </summary>
    public class CameraController : MonoBehaviour {
        // How high you can move your camera.
        [SerializeField, Range(0f, 90f)] private float upperVerticalLimit = 89f;
        // How low you can move your camera.
        [SerializeField, Range(0f, 90f)] private float lowerVerticalLimit = 89f;
        // How fast you can move your camera.
        [SerializeField] private float cameraSpeed = 25f;
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
        [SerializeField] private CameraRigManager cameraRig;
        [SerializeField] private Helper.CameraCouplingMode playerRotationMode;
        
        private CinemachineBrain _brain;
        private Transform _tr;
        // Cached local rotation value X.
        private float _currentXAngle;
        // Cached local rotation value Y.
        private float _currentYAngle;

        [SerializeField] private Transform cameraPivot;
        
        private void Awake() {
            _tr = transform;
            
            if (input == null)
                Debug.LogError("CameraController: Drag and drop an input SO in.", this);
            
            if (!_brain && Camera.main) 
                Camera.main.TryGetComponent(out _brain);
            
            _currentXAngle = _tr.localRotation.eulerAngles.x;
            _currentYAngle = _tr.localRotation.eulerAngles.y;
        }

        private void OnEnable() {
            CameraSetup();

            if (cameraRig)
                input.OnMouseWheelInput += ZoomCamera;
        }

        private void OnDisable() {
            if (cameraRig)
                input.OnMouseWheelInput -= ZoomCamera;
        }

        private void Start() {
            Cursor.lockState = cursorLockOnStart
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;
        }

        private void Update() {
            RotateCamera(input.LookDirection.x, -input.LookDirection.y);
        }

        public Vector3 GetUpDirection() => _tr.up;
        public Vector3 GetFacingDirection() => _tr.forward;
        
        /// <summary>
        /// Rotates the camera based on device horizontal and vertical input about the pivot.
        /// </summary>
        private void RotateCamera(float horizontalInput, float verticalInput) {
            var targetX = _currentXAngle + verticalInput  * cameraSpeed * Time.deltaTime;
            var targetY = _currentYAngle + horizontalInput * cameraSpeed * Time.deltaTime;

            targetX = Mathf.Clamp(targetX, -upperVerticalLimit, lowerVerticalLimit);

            if (smoothCameraRotation) {
                _currentXAngle = Mathf.LerpAngle(_currentXAngle, targetX, cameraSmoothingFactor * Time.deltaTime);
                _currentYAngle = Mathf.LerpAngle(_currentYAngle, targetY, cameraSmoothingFactor * Time.deltaTime);
            } else {
                _currentXAngle = targetX;
                _currentYAngle = targetY;
            }

            //var yaw   = Quaternion.AngleAxis(_currentYAngle, Vector3.up);
            //var pitch = Quaternion.AngleAxis(_currentXAngle, Vector3.right);
            //cameraPivot.localRotation = Quaternion.AngleAxis(_currentXAngle, yaw * Vector3.right) * yaw;

            //_currentXAngle += verticalInput * cameraSpeed * Time.deltaTime;
            //_currentYAngle += horizontalInput * cameraSpeed * Time.deltaTime;

            _currentXAngle = Mathf.Clamp(_currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
            cameraPivot.localRotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0f);
            //cameraPivot.localRotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0);
        }

        private void ZoomCamera(Vector2 zoomInput) {
            var currentZoom = cameraRig.CurrentCameraZoom();

            if (float.IsNaN(currentZoom))
                return;

            var target = currentZoom - zoomInput.y * zoomIncrement;
            target = Mathf.Clamp(target, minZoomDistance, maxZoomDistance);
            cameraRig.SetCameraZoom(target);
        }
        
        /// <summary>
        /// Sets our cameraRig tracking target.
        /// </summary>
        private void CameraSetup() {
            _brain.WorldUpOverride = cameraPivot;
            
            cameraRig = _brain.ActiveVirtualCamera as CameraRigManager;

            if (cameraRig == null) {
                Debug.LogError("Camera rig is null and doesn't appear to be in scene.");
                return;
            }

            cameraRig.DefaultTarget.Target.TrackingTarget = cameraPivot;
        }
    }
}