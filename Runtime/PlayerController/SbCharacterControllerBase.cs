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
    public abstract class SbCharacterControllerBase : MonoBehaviour {
        [Header("References")]
        [SerializeField] public PlayerInputActionsSO input;
        [SerializeField] private Transform referenceTransform;
        
        [Header("Settings")]
        [SerializeField] private float turnTowardsInputSpeed = 500f;
        
        [Header("Default Values")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float airControlRate = 2f;
        [SerializeField] private float jumpSpeed = 10f;
        [SerializeField] private float jumpDuration = 0.2f;
        [SerializeField] private float airFriction = 0.5f;
        [SerializeField] private float groundFriction = 100f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float slideGravity = 5f;
        [SerializeField] private float slopeLimit = 30f;

        private const float RotationFallOffAngle = 90f;
        
        private RigidbodyMover _rigidbodyMover;
        private LocoStateMachine _locoStateMachine;
        private ActionStateMachine _actionStateMachine;
        private AnimationControllerBase _animator;
        
        [Header("State Debugging")]
        [SerializeField] private BaseLocoStateSO currentLocoState;
        [SerializeField] private BaseActionStateSO currentActionState;

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
        private Vector3 _velocity;
        private Vector3 _planarUp;
        
        // #######################################
        // State Polling --Condensed Value Types--
        // #######################################
        public float horizontalSpeed;
        public bool jumpFlag;
        public bool GroundFlag => _rigidbodyMover.IsGrounded();
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
                InputManager.Instance.GetInputs();
            
            _rigidbodyMover = GetComponent<RigidbodyMover>();
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
            _locoStateMachine.CurrentLocoStateDriver.FixedUpdateState();
            _actionStateMachine.CurrentActionStateDriver.FixedUpdateState();
            
            HandleCharacterTurnTowardsHorizontalVelocity();
        }
        
        public void HandleHorizontalVelocityInput() {
            // Handles additional vertical velocity if necessary.
            _rigidbodyMover.CheckForGround();
            
            var velocity = CalculateMovementVelocity();
            velocity.y += _rigidbodyMover.GetRigidbodyVelocity().y;
            
            _rigidbodyMover.SetVelocity(velocity);
            
            _velocity = velocity;
            horizontalSpeed = Vector3.ProjectOnPlane(_velocity, _planarUp).magnitude;
        }

        private void HandleCharacterTurnTowardsHorizontalVelocity() {
            var planarVelocity = Vector3.ProjectOnPlane(
                    _rigidbodyMover.GetRigidbodyVelocity(), _planarUp);

            if (planarVelocity.sqrMagnitude < 1e-6f)
                return;

            var desiredDir = planarVelocity.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDir, _planarUp);
            var angleDiff = Quaternion.Angle(_rigidbodyMover.GetRigidbodyRotation(), targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, RotationFallOffAngle, angleDiff);
            
            var maxStepDeg = turnTowardsInputSpeed * speedFactor * Time.fixedDeltaTime;

            var nextRotation = Quaternion.RotateTowards(
                    _rigidbodyMover.GetRigidbodyRotation(), targetRotation, maxStepDeg);
            
            _rigidbodyMover.SetRigidbodyRotation(nextRotation);
        }

        /// <summary>
        /// Returns the desired velocity vector.
        /// </summary>
        /// <returns></returns>
        private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;
        
        /// <summary>
        /// Returns a direction that the player is moving.
        /// </summary>
        private Vector3 CalculateMovementDirection() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                              referenceTransform.right, _tr.up).normalized * 
                      input.Direction.x + 
                      Vector3.ProjectOnPlane(
                              referenceTransform.forward, _tr.up).normalized * 
                      input.Direction.y;
            
            return direction.magnitude > 1f 
                    ? direction.normalized 
                    : direction;
        }

        private void HandleLocoStateChanged(BaseLocoStateSO state) => currentLocoState = state;
        private void HandleActionStateChanged(BaseActionStateSO state) => currentActionState = state;
        
        public Transform GetReferenceTransform() => referenceTransform;
        
        public void SetSensorRange(Helper.RaycastLength sensorLength) {
            switch (sensorLength) {
                case Helper.RaycastLength.Normal:
                    _rigidbodyMover.SetSensorRange(1);
                    break;
                case Helper.RaycastLength.Extended:
                    _rigidbodyMover.SetSensorRange(1.1f);
                    break;
                case Helper.RaycastLength.Retracted:
                    _rigidbodyMover.SetSensorRange(0.5f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorLength), sensorLength, null);
            }
        }

        #region StateEvaluaters
        private void HandleJumpPressed() {
            if (!_rigidbodyMover.IsGrounded())
                return;

            if (!ResourceCheck())
                return;
            
            jumpFlag = true;
        }

        public void Jump() {
            _rigidbodyMover.ApplyJumpForce(5f);
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
    }
}