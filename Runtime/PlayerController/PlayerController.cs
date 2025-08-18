using UnityEngine;
using SpellBound.Controller.PlayerInputs;
using SpellBound.Controller.ManagersAndStatics;
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
        [SerializeField] private bool useLocalMomentum;

        private RigidbodyMover _rigidbodyMover;
        private Transform _tr;
        private Vector3 _momentum;
        private Vector3 _savedVelocity;
        private Vector3 _savedMovementVelocity;
        private Vector3 _planarUp;
        private float _currentYRotation;
        
        private const float FallOffAngle = 90f;
        
        private void Awake() {
            _tr = transform;
            _planarUp = _tr.up;
            
            if (input == null)
                Debug.LogError("PlayerController: Drag and drop an input SO in.", this);
            
            _rigidbodyMover = GetComponent<RigidbodyMover>();
        }

        private void Start() {
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;
            _currentYRotation = _tr.eulerAngles.y;
        }

        private void FixedUpdate() {
            _rigidbodyMover.CheckForGround();
            // HandleMomentum();
            var velocity = CalculateMovementVelocity();
            velocity += useLocalMomentum ? _tr.localToWorldMatrix * _momentum : _momentum;
            
            _rigidbodyMover.SetExtendSensorRange(true);
            _rigidbodyMover.SetVelocity(velocity);

            _savedVelocity = velocity;
            _savedMovementVelocity = CalculateMovementVelocity();
        }

        private void LateUpdate() {
            #region TurnTowardsInput
            // Basically gives the x,z components of our velocity vector since they are normal to the up direction.
            var velocity = Vector3.ProjectOnPlane(
                    GetMovementVelocity(), _planarUp);
            
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
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                              referenceTransform.right, _tr.up).normalized * 
                      input.Direction.x + 
                      Vector3.ProjectOnPlane(
                              referenceTransform.forward, _tr.up).normalized * 
                      input.Direction.y;
            
            return direction.magnitude > 1f ? direction.normalized : direction;
        }
    }
}