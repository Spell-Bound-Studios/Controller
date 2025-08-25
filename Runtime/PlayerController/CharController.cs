using System;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;
using SpellBound.Controller.PlayerInputs;
using SpellBound.Controller.ManagersAndStatics;
using SpellBound.Controller.PlayerStateMachine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Input and stats meet here to inform supporting members.
    /// </summary>
    [RequireComponent(typeof(RigidbodyMover))]
    public class CharController : MonoBehaviour {
        [Header("References")]
        [SerializeField] public PlayerInputActionsSO input;
        [SerializeField] private Transform referenceTransform;
        [SerializeField] private NetworkAnimator animator;
        
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
        private AnimationController _animationController;
        
        // Visuals - never used.
        [SerializeField] private BaseLocoStateSO currentLocoState;
        [SerializeField] private BaseActionStateSO currentActionState;

        private readonly List<string> _defaultLocoStatesList = new() {
                StateHelper.DefaultGroundStateSO,
                StateHelper.DefaultFallingStateSO,
                StateHelper.DefaultJumpingStateSO,
                StateHelper.DefaultLandingStateSO,
        };

        private readonly List<string> _defaultActionStatesList = new() {
                StateHelper.DefaultReadyStateSO
        };
        
        private Transform _tr;
        private Vector3 _momentum;
        private Vector3 _velocity;
        private Vector3 _planarUp;
        private float _currentYRotation;
        
        // State Polling --Condensed Value Types--
        public float horizontalSpeed;
        public bool jumpFlag;
        public bool GroundFlag => _rigidbodyMover.IsGrounded();
        
        private void Awake() {
            _tr = transform;
            _planarUp = _tr.up;
            
            if (input == null)
                Debug.LogError("PlayerController: Drag and drop an input SO in.", this);
            
            _rigidbodyMover = GetComponent<RigidbodyMover>();

            if (animator != null) 
                return;

            Debug.LogError("PlayerController: Drag and drop animator component in.", this);
            animator = GetComponentInChildren<NetworkAnimator>();
        }

        private void OnEnable() {
            StateHelper.OnLocoStateChange += HandleLocoStateChanged;
            StateHelper.OnActionStateChange += HandleActionStateChanged;

            if (input) {
                input.OnJumpInput += HandleJumpPressed;
            }
        }

        private void OnDisable() {
            StateHelper.OnLocoStateChange -= HandleLocoStateChanged;
            StateHelper.OnActionStateChange -= HandleActionStateChanged;

            _animationController?.DisposeEvents();
            
            if (input) {
                input.OnJumpInput -= HandleJumpPressed;
            }
        }

        private void Start() {
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;
            _currentYRotation = _tr.eulerAngles.y;
            
            _animationController = new AnimationController(animator);
            
            _locoStateMachine = new LocoStateMachine(this, _defaultLocoStatesList);
            _actionStateMachine = new ActionStateMachine(this, _defaultActionStatesList);
        }

        private void Update() {
            _locoStateMachine.CurrentLocoStateDriver.UpdateState();
        }
        
        private void FixedUpdate() {
            _locoStateMachine.CurrentLocoStateDriver.FixedUpdateState();
        }

        private void LateUpdate() {
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
            // Basically gives the x,z components of our velocity vector since they are normal to the up direction.
            var velocity = Vector3.ProjectOnPlane(_velocity, _planarUp);
            
            // Return early if we're not moving.
            if (velocity.magnitude < 0.001f)
                return;
            
            var desiredFacingDir = velocity.normalized;
            
            var angleDiff = Helper.GetAngle(_tr.forward, desiredFacingDir, _planarUp);

            var step = Mathf.Sign(angleDiff) *
                       Mathf.InverseLerp(0f, RotationFallOffAngle, Mathf.Abs(angleDiff)) *
                       Time.deltaTime * turnTowardsInputSpeed;
            
            _currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDiff) ? angleDiff : step;
            _tr.localRotation = Quaternion.Euler(0, _currentYRotation, 0);
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
    }
}