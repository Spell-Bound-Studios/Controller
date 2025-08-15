using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    /// <summary>
    /// Input and stats meet here to inform supporting members.
    /// </summary>
    [RequireComponent(typeof(PlayerMover))]
    public class PlayerController : MonoBehaviour {
        [SerializeField] private PlayerInputActionsSO input;
        private PlayerMover _playerMover;
        private Transform _cameraTransform;
        private Transform _tr;

        public float movementSpeed = 5f;
        public float airControlRate = 2f;
        public float jumpSpeed = 10f;
        public float jumpDuration = 0.2f;
        public float airFriction = 0.5f;
        public float groundFriction = 100f;
        public float gravity = 30f;
        public float slideGravity = 5f;
        public float slopeLimit = 30f;
        public bool useLocalMomentum;

        private Vector3 _momentum;
        private Vector3 _savedVelocity;
        private Vector3 _savedMovementVelocity;
        
        private void Awake() {
            _tr = transform;

            if (input == null)
                Debug.LogError("PlayerController: Drag and drop an input SO in.", this);
            
            _playerMover = GetComponent<PlayerMover>();
        }

        private void FixedUpdate() {
            _playerMover.CheckForGround();
            HandleMomentum();
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
            var direction = _cameraTransform == null
                    ? _tr.right * input.Direction.x + _tr.forward * input.Direction.y
                    : Vector3.ProjectOnPlane(_cameraTransform.right, _tr.up).normalized * input.Direction.x +
                      Vector3.ProjectOnPlane(_cameraTransform.forward, _tr.up).normalized * input.Direction.y;
            
            return direction.magnitude > 1f ? direction.normalized : direction;
        }
    }
}