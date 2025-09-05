using System;
using System.Collections.Generic;
using SpellBound.Controller.ManagersAndStatics;
using SpellBound.Controller.PlayerInputs;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Input and stats meet here to inform supporting members.
    /// </summary>
    [RequireComponent(typeof(RigidbodyMover))]
    public abstract class SbCharacterControllerBase : MonoBehaviour, IDebuggingInfo {
        [Header("References")]
        [SerializeField] public PlayerInputActionsSO input;
        [SerializeField] private Transform referenceTransform;
        
        [Header("Settings")]
        [SerializeField] private float turnTowardsInputSpeed = 500f;
        
        [Header("Default Values")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float airControlRate = 2f;
        [SerializeField] private float jumpSpeed = 10f;
        [SerializeField] private float jumpForce = 5f;
        [SerializeField] private float airFriction = 0.5f;
        [SerializeField] private float groundFriction = 100f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float slideGravity = 5f;

        private const float RotationFallOffAngle = 90f;
        
        private RigidbodyMover _mover;
        private LocoStateMachine _locoStateMachine;
        private ActionStateMachine _actionStateMachine;
        private AnimationControllerBase _animator;
        
        private BaseLocoStateSO _currentLocoState;
        private BaseActionStateSO _currentActionState;
        
        private readonly List<string> _defaultLocoStatesList = new() {
                StateHelper.DefaultGroundStateSO,
                StateHelper.DefaultFallingStateSO,
                StateHelper.DefaultJumpingStateSO,
                StateHelper.DefaultLandingStateSO,
        };

        private readonly List<string> _defaultActionStatesList = new() {
                StateHelper.DefaultReadyStateSO,
                StateHelper.DefaultGCDStateSO,
                StateHelper.DefaultInteractStateSO,
        };
        
        private Transform _tr;
        private Vector3 _horizontalVelocity;
        private Vector3 _planarUp;
        
        // #######################################
        // State Polling --Condensed Value Types--
        // #######################################
        public float horizontalSpeed;
        public bool jumpFlag;
        public bool GroundFlag => _mover.IsGrounded();
        public bool hotkeyOneFlagged;
        public bool interactFlagged;
        
        /// <summary>
        /// Derived classes must provide the correct animation controller.
        /// </summary>
        protected abstract AnimationControllerBase CreateAnimationController();
        
        private void Awake() {
            _tr = transform;
            _planarUp = _tr.up;
            
            if (input == null)
                Debug.LogError("Please drag and drop an input reference in the CharacterController", this);
            
            _mover = GetComponent<RigidbodyMover>();
        }
        
        private void OnEnable() {
            StateHelper.OnLocoStateChange += HandleLocoStateChanged;
            StateHelper.OnActionStateChange += HandleActionStateChanged;
            
            if (!input) 
                return;

            input.OnJumpInput += HandleJumpPressed;
            input.OnInteractPressed += HandleInteractPressed;
            input.OnHotkeyOnePressed += HandleHotkeyOnePressed;
        }

        private void OnDisable() {
            StateHelper.OnLocoStateChange -= HandleLocoStateChanged;
            StateHelper.OnActionStateChange -= HandleActionStateChanged;
            
            _animator?.DisposeEvents();

            if (!input) 
                return;

            input.OnJumpInput -= HandleJumpPressed;
            input.OnInteractPressed -= HandleInteractPressed;
            input.OnHotkeyOnePressed -= HandleHotkeyOnePressed;
        }
        
        private void Start() {
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;

            _animator = CreateAnimationController();
            
            if (_animator == null)
                Debug.LogError("CharacterControllerBase requires an animator via the CreateAnimationController override.", 
                        this);
            
            _locoStateMachine = new LocoStateMachine(this, _defaultLocoStatesList);
            _actionStateMachine = new ActionStateMachine(this, _defaultActionStatesList);
        }
        
        private void Update() {
            _locoStateMachine.CurrentLocoStateDriver.UpdateState();
            _actionStateMachine.CurrentActionStateDriver.UpdateState();
        }
        
        private void FixedUpdate() {
            // Drives the ground sensor and states will read the result.
            _mover.RunGroundSensor();
            
            _locoStateMachine.CurrentLocoStateDriver.FixedUpdateState();
            _actionStateMachine.CurrentActionStateDriver.FixedUpdateState();
            
            HandleCharacterTurnTowardsHorizontalVelocity();
        }
        
        private void HandleLocoStateChanged(BaseLocoStateSO state) => _currentLocoState = state;
        private void HandleActionStateChanged(BaseActionStateSO state) => _currentActionState = state;
        
        public void HandleInput(float hSpeedModifier, float vSpeedModifier) {
            var inputDesired = CalculateCameraRelativeMovementFromInput();
            var weightForce = _mover.GetMass() * gravity;
            
            var currentHorizontalVelocity = _mover.GetRigidbodyVelocity();
            currentHorizontalVelocity.y = 0;
            
            _horizontalVelocity = hSpeedModifier * movementSpeed * inputDesired - currentHorizontalVelocity;
            _mover.ApplyForce(_horizontalVelocity);
            
            // Calculate the actual horizontal speed at the end so that we can send to animator.
            horizontalSpeed = Vector3.ProjectOnPlane(currentHorizontalVelocity, _planarUp).magnitude;
        }

        public void HandleHorizontalForceInput() {
            _mover.RunGroundSensor();

            _mover.SetLinearDampening(_mover.IsGrounded() 
                    ? 10 
                    : 0);

            var direction = CalculateCameraRelativeMovementFromInput();
            
            _mover.ApplyForce(direction * 80f);
        }

        private void HandleCharacterTurnTowardsHorizontalVelocity() {
            var planarVelocity = Vector3.ProjectOnPlane(
                    _mover.GetRigidbodyVelocity(), _planarUp);

            if (planarVelocity.sqrMagnitude < 1e-6f)
                return;

            var desiredDir = planarVelocity.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDir, _planarUp);
            var angleDiff = Quaternion.Angle(_mover.GetRigidbodyRotation(), targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, RotationFallOffAngle, angleDiff);
            
            var maxStepDeg = turnTowardsInputSpeed * speedFactor * Time.fixedDeltaTime;

            var nextRotation = Quaternion.RotateTowards(
                    _mover.GetRigidbodyRotation(), targetRotation, maxStepDeg);
            
            _mover.SetRigidbodyRotation(nextRotation);
        }
        
        /// <summary>
        /// Returns a horizontal movement vector from camera-relative input.
        /// </summary>
        private Vector3 CalculateCameraRelativeMovementFromInput() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                              referenceTransform.right, _planarUp).normalized * 
                      input.Direction.x + 
                      Vector3.ProjectOnPlane(
                              referenceTransform.forward, _planarUp).normalized * 
                      input.Direction.y;
            
            return direction.magnitude > 1f 
                    ? direction.normalized 
                    : direction;
        }
        
        public Transform GetReferenceTransform() => referenceTransform;
        
        public void SetSensorRange(Helper.RaycastLength sensorLength) {
            switch (sensorLength) {
                case Helper.RaycastLength.Normal:
                    _mover.SetSensorRange(1);
                    break;
                case Helper.RaycastLength.Extended:
                    _mover.SetSensorRange(1.1f);
                    break;
                case Helper.RaycastLength.Retracted:
                    _mover.SetSensorRange(0.5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorLength), sensorLength, null);
            }
        }

        #region StateEvaluaters
        private void HandleJumpPressed() {
            if (!_mover.IsGrounded())
                return;

            if (!ResourceCheck())
                return;
            
            jumpFlag = true;
        }

        public void Jump() {
            _mover.ApplyJumpForce(jumpForce);
        }

        private bool ResourceCheck() {
            return true;
        }
        
        private void HandleInteractPressed() {
            if (interactFlagged)
                return;
            
            interactFlagged = true;
        }

        private void HandleHotkeyOnePressed() {
            if (hotkeyOneFlagged)
                return;
            
            if (!Physics.Raycast(
                        referenceTransform.position,
                        input.LookDirection,
                        out var hit,
                        10f,
                        1 << 6,
                        QueryTriggerInteraction.Ignore))
                return;

            hotkeyOneFlagged = true;
        }
        #endregion

        /// <summary>
        /// Runs if the debugger is attached.
        /// </summary>
        public void RegisterDebugInfo(SbPlayerDebugHudBase hud) {
            hud.Field("Controller.LocoState", () => {
                var lsName = _currentLocoState ? _currentLocoState.name : "-";
                return lsName;
            });
            
            hud.Field("Controller.ActionState", () => {
                var asName = _currentActionState ? _currentActionState.name : "-";
                return asName;
            });
            
            hud.Field("Controller.HorizontalSpeed", () => {
                if (_mover == null) 
                    return "-";
                
                var v = _mover.GetRigidbodyVelocity();
                var hMag = Vector3.ProjectOnPlane(v, _planarUp).magnitude;
                return hMag.ToString("F2");
            });
            
            hud.Field("Controller.VerticalSpeed", () => {
                if (_mover == null) 
                    return "-";
                
                var v = _mover.GetRigidbodyVelocity();
                var vMag = Vector3.Dot(v, _planarUp);
                return vMag.ToString("F2");
            });

            // Jump force (current configured value)
            hud.Field("Controller.JumpForce", () => jumpForce.ToString("F2"));
        }
    }
}