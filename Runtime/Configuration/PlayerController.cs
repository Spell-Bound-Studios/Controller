using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    /// <summary>
    /// Input and stats meet here to inform supporting members.
    /// </summary>
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerController : MonoBehaviour {
        [SerializeField] private PlayerInputActionsSO input;
        [SerializeField] private Transform referenceTransform;
        
        [Header("Default values.")]
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private float airControlRate = 2f;
        [SerializeField] private float jumpSpeed = 10f;
        [SerializeField] private float jumpDuration = 0.2f;
        [SerializeField] private float airFriction = 0.5f;
        [SerializeField] private float groundFriction = 100f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float slideGravity = 5f;
        [SerializeField] private float slopeLimit = 30f;
        [SerializeField] private bool useLocalMomentum;

        private PlayerMover _playerMover;
        
        private Transform _tr;
        
        private Vector3 _momentum;
        private Vector3 _savedVelocity;
        private Vector3 _savedMovementVelocity;
        
        private void Awake() {
            _tr = transform;

            if (input == null)
                Debug.LogError("PlayerController: Drag and drop an input SO in.", this);
            
            _playerMover = GetComponent<PlayerMover>();
        }

        private void Start() {
            if (referenceTransform == null)
                Debug.LogWarning("[PlayerController] the reference transform is null. Using default transform and may " +
                                 "result in odd behaviour.");
        }

        private void FixedUpdate() {
            _playerMover.CheckForGround();
            // HandleMomentum();
            var velocity = CalculateMovementVelocity();
            velocity += useLocalMomentum ? _tr.localToWorldMatrix * _momentum : _momentum;
            
            _playerMover.SetExtendSensorRange(true);
            _playerMover.SetVelocity(velocity);

            _savedVelocity = velocity;
            _savedMovementVelocity = CalculateMovementVelocity();
        }
        
        public Vector3 GetMomentum() => useLocalMomentum ? _tr.localToWorldMatrix * _momentum : _momentum;

        private void HandleMomentum() {
            if (useLocalMomentum)
                _momentum = _tr.localToWorldMatrix * _momentum;

            var verticalMomentum = Helper.ExtractDotVector(_momentum, _tr.up);
            var horizontalMomentum = _momentum - verticalMomentum;

            verticalMomentum -= _tr.up * (gravity * Time.deltaTime);
            // Likely have state reference here.
            
        }

        private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;
        public Vector3 GetMovementVelocity() => _savedMovementVelocity;
        
        private Vector3 CalculateMovementDirection() {
            var direction = referenceTransform == null
                    ? _tr.right * input.Direction.x + _tr.forward * input.Direction.y
                    : Vector3.ProjectOnPlane(
                              referenceTransform.right, referenceTransform.up).normalized * 
                      input.Direction.x + 
                      Vector3.ProjectOnPlane(
                              referenceTransform.forward, referenceTransform.up).normalized * 
                      input.Direction.y;
            
            return direction.magnitude > 1f ? direction.normalized : direction;
        }
    }
}