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
    public class PlayerController : MonoBehaviour {
        [Header("References")]
        [SerializeField] private PlayerInputActionsSO input;
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
        // This will be momentum given by your inputs.
        [SerializeField] private bool useLocalMomentum;

        private RigidbodyMover _rigidbodyMover;
        private LocoStateMachine _locoStateMachine;
        private ActionStateMachine _actionStateMachine;
        private AnimationController _animationController;
        
        [SerializeField] private BaseLocoStateSO currentLocoState;
        //[SerializeField] private BaseStateSO baseLocoStateSO;
        
        private LocoStateContext _locoCtx;

        private readonly List<string> _defaultStatesList = new() {
                StateHelper.DefaultGroundStateSO,
                StateHelper.DefaultFallingStateSO
        };
        
        private Transform _tr;
        private Vector3 _momentum;
        private Vector3 _velocity;
        private Vector3 _planarUp;
        private float _currentYRotation;
        
        private const float FallOffAngle = 90f;
        
        private void Awake() {
            _tr = transform;
            _planarUp = _tr.up;
            
            if (input == null)
                Debug.LogError("PlayerController: Drag and drop an input SO in.", this);
            
            _rigidbodyMover = GetComponent<RigidbodyMover>();

            if (animator == null) {
                Debug.LogError("PlayerController: Drag and drop animator component in.", this);
                animator = GetComponentInChildren<NetworkAnimator>();
            }
                
        }

        private void OnEnable() {
            StateHelper.OnLocoStateChange += HandleLocoStateChanged;
        }

        private void OnDisable() {
            StateHelper.OnLocoStateChange -= HandleLocoStateChanged;

            _animationController?.DisposeEvents();
        }

        private void Start() {
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;
            _currentYRotation = _tr.eulerAngles.y;
            
            _animationController = new AnimationController(animator);
            
            _locoStateMachine = new LocoStateMachine(_defaultStatesList);
            _actionStateMachine = new ActionStateMachine();
        }

        private void Update() {
            _locoStateMachine.CurrentLocoStateDriver.UpdateState();
        }

        private void FixedUpdate() {
            #region ClassicalMechanics
            // Current velocity of the rigidbody.
            var rbVelocity = _rigidbodyMover.GetRigidbodyVelocity();
            
            // Handles additional vertical velocity if necessary.
            _rigidbodyMover.CheckForGround();
            
            var velocity = CalculateMovementVelocity();
            velocity += useLocalMomentum ? _tr.localToWorldMatrix * _momentum : _momentum;
            
            _rigidbodyMover.SetExtendSensorRange(true);
            _rigidbodyMover.SetVelocity(velocity);

            _velocity = velocity;
            #endregion
            
            #region StateCtx
            // Capture state values this frame and then pass in to the state machine for deterministic state context.
            _locoCtx.MoveInput = new Vector2(input.Direction.x, input.Direction.y);
            _locoCtx.Speed = _velocity.magnitude;

            _locoStateMachine.SetContext(in _locoCtx);
            _locoStateMachine.CurrentLocoStateDriver.FixedUpdateState();
            #endregion
        }

        private void LateUpdate() {
            #region TurnTowardsInput
            // Basically gives the x,z components of our velocity vector since they are normal to the up direction.
            var velocity = Vector3.ProjectOnPlane(
                    _velocity, _planarUp);
            
            // Return early if we're not moving.
            if (velocity.magnitude < 0.001f)
                return;
            
            var desiredFacingDir = velocity.normalized;
            
            var angleDiff = Helper.GetAngle(_tr.forward, desiredFacingDir, _planarUp);

            var step = Mathf.Sign(angleDiff) *
                       Mathf.InverseLerp(0f, FallOffAngle, Mathf.Abs(angleDiff)) *
                       Time.deltaTime * turnTowardsInputSpeed;
            
            _currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDiff) ? angleDiff : step;
            _tr.localRotation = Quaternion.Euler(0, _currentYRotation, 0);
            #endregion
        }

        private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;
        
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
    }
}