using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    public class Controller : MonoBehaviour {
        private CinemachineBrain _brain;
        private InputActions _inputActions;
        private Rigidbody _rigidbody;
        private Vector2 _moveInput;
        
        [SerializeField] private CinemachineCameraManagerBase cameraRig;
        [SerializeField] private Helper.CameraCouplingMode playerRotationMode;
        
        private void Awake() {
            Debug.Log("Controller is awake.");
            
            if (!_brain && Camera.main) 
                Camera.main.TryGetComponent(out _brain);
            
            if (!_brain)
                Debug.LogError("No Cinemachine Brain found on the main camera.", this);
            
            if (!_rigidbody)
                _rigidbody = GetComponent<Rigidbody>();
            
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.freezeRotation = true;
            
            _inputActions = new InputActions();
        }

        private void OnEnable() {
            _inputActions.Enable();
            
            // Movement (WASD)
            _inputActions.PlayerInput.Movement.performed += OnMovePerformed;
            _inputActions.PlayerInput.Movement.canceled += OnMoveCanceled;
            
            // Jump (space bar)
            _inputActions.PlayerInput.Jump.performed += _ => OnJumpInput();
        }

        private void OnDisable() {
            _inputActions.Disable();
        }

        private void Start() {
            CameraSetup();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void FixedUpdate() {
            _rigidbody.linearVelocity = new Vector3(_moveInput.x, 0, _moveInput.y);
        }

        private void CameraSetup() {
            cameraRig = _brain.ActiveVirtualCamera as CinemachineCameraManagerBase;

            if (cameraRig == null) {
                Debug.LogError("Camera rig is null and doesn't appear to be in scene.");
                return;
            }

            cameraRig.DefaultTarget.Target.TrackingTarget = transform;
        }
        
        private void OnMovePerformed(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
        private void OnMoveCanceled(InputAction.CallbackContext ctx) => _moveInput = Vector2.zero;
        
        private void OnJumpInput() {
            Debug.Log("Jumping.");
        }
    }
}