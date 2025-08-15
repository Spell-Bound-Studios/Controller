using Unity.Cinemachine;
using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    /// <summary>
    /// Interface for how the player inputs drive its members.
    /// </summary>
    public class CameraController : MonoBehaviour {
        [SerializeField, Range(0f, 90f)] private float upperVerticalLimit = 89f;
        [SerializeField, Range(0f, 90f)] private float lowerVerticalLimit = 89f;
        [SerializeField] private float cameraSpeed = 25f;
        [SerializeField] private bool smoothCameraRotation;
        [SerializeField, Range(1f, 50f)] private float cameraSmoothingFactor = 25f;
        [SerializeField, Min(0.1f)] private float minZoomDistance = 1f;
        [SerializeField, Min(1f)] private float maxZoomDistance = 8f;
        [SerializeField] private float zoomIncrement = .2f;
        
        [SerializeField] private bool cursorLockOnStart = true;
        [SerializeField] private PlayerInputActionsSO input;
        [SerializeField] private CameraRigManager cameraRig;
        [SerializeField] private Helper.CameraCouplingMode playerRotationMode;
        
        private CinemachineBrain _brain;
        private Transform _tr;
        private float _currentXAngle;
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
            input.OnMouseWheelInput += ZoomCamera;
        }

        private void OnDisable() {
            input.OnMouseWheelInput -= ZoomCamera;
        }

        private void Start() {
            CameraSetup();

            Cursor.lockState = cursorLockOnStart
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;
        }

        private void LateUpdate() {
            RotateCamera(input.LookDirection.x, -input.LookDirection.y);
        }

        public Vector3 GetUpDirection() => _tr.up;
        public Vector3 GetFacingDirection() => _tr.forward;
        
        /// <summary>
        /// Rotates the camera based on device horizontal and vertical input.
        /// </summary>
        private void RotateCamera(float horizontalInput, float verticalInput) {
            if (smoothCameraRotation) {
                horizontalInput = Mathf.Lerp(0, horizontalInput, Time.deltaTime * cameraSmoothingFactor);
                verticalInput = Mathf.Lerp(0, verticalInput, Time.deltaTime * cameraSmoothingFactor);
            }

            _currentXAngle += verticalInput * cameraSpeed * Time.deltaTime;
            _currentYAngle += horizontalInput * cameraSpeed * Time.deltaTime;

            _currentXAngle = Mathf.Clamp(_currentXAngle, -upperVerticalLimit, lowerVerticalLimit);
            
            cameraPivot.localRotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0);
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
            cameraRig = _brain.ActiveVirtualCamera as CameraRigManager;

            if (cameraRig == null) {
                Debug.LogError("Camera rig is null and doesn't appear to be in scene.");
                return;
            }

            cameraRig.DefaultTarget.Target.TrackingTarget = cameraPivot;
        }
    }
}