using UnityEngine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Interface for rigidbody and collider elements. Therefore, anything that acts on either should be handled here.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class RigidbodyMover : MonoBehaviour {
        [Header("Collider Settings:")] 
        [SerializeField, Range(0f, 1f)] private float stepHeightRatio = 0.1f;
        [SerializeField] private float colliderHeight = 2f;
        [SerializeField] private float colliderDiameter = 1f;
        [SerializeField] private Vector3 colliderOffset = new(0, .4f, 0);

        [Header("Rigidbody Settings:")]
        [SerializeField] private ForceMode horizontalForceMode = ForceMode.Force;
        [SerializeField] private ForceMode verticalForceMode = ForceMode.Impulse;
        
        [Header("Sensor Settings:")]
        [SerializeField] private float inclineGroundTolerance = 60f;

        private Transform _tr;
        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private RaycastSensor _raycastSensor;

        private bool _isGrounded;
        private bool _isSliding;
        private float _baseSensorRange;
        private float _adjustedSensorRange;
        private Vector3 _currentGroundAdjustmentVelocity;
        private int _currentLayer;
        private float _colliderHalfSize;

        private void Awake() {
            _tr = transform;
            Setup();
            RecalculateColliderDimensions();
        }

        private void OnValidate() {
            _tr = transform;
            
            if (gameObject.activeInHierarchy)
                RecalculateColliderDimensions();
        }
        
        /// <summary>
        /// Checks for the ground based on layer and raycast.
        /// Returns early if you're not on the ground.
        /// </summary>
        public void CheckForGround() {
            if (_currentLayer != gameObject.layer)
                RecalculateSensorLayerMask();

            _currentGroundAdjustmentVelocity = Vector3.zero;
            
            // Extends the sensor range if we are using step-up logic but otherwise uses the base range.
            _raycastSensor.CastLength = _adjustedSensorRange;
            
            _raycastSensor.SphereRadius = _collider.radius;
            _raycastSensor.SphereCastLength = _colliderHalfSize + stepHeightRatio;
            
            _raycastSensor.CastRaycast();
            
            var detectedHit = _raycastSensor.HasDetectedHit();
            var groundNormal = _raycastSensor.GetNormal();

            if (Vector3.Angle(groundNormal, Vector3.up) > inclineGroundTolerance) {
                _isGrounded = false;
                _isSliding = true;
                return;
            }

            _isGrounded = detectedHit;

            if (!_isGrounded)
                return;
            
            // The following attempts to tackle quantifying how much we should shove the player up or down based on their
            // collider. Imagine moving through a very bumpy region: we don't want the character to briefly enter the
            // falling state every few steps... so this will attempt to quantify a value to push them up or down and keep
            // them flush to the ground when they are within tolerance.
            
            // Distance from the ground.
            var distance = _raycastSensor.GetRaycastHitDistance();
            // Top boundary of where the player should be positioned.
            var upperLimit = colliderHeight * _tr.localScale.x * (1f - stepHeightRatio) * 0.5f;
            // Where feet should be relative to the ground.
            var middle = upperLimit + colliderHeight * _tr.localScale.x * stepHeightRatio;
            // Difference between where the player is and where they should be: the middle.
            var distanceToGo = middle - distance;
            // Velocity needs to move the player to the correct position.
            //_currentGroundAdjustmentVelocity = _tr.up * (distanceToGo / Time.fixedDeltaTime);
        }

        public bool IsGrounded() => _isGrounded;
        public bool IsSliding() => _isSliding;
        public Vector3 GetGroundNormal() => _raycastSensor.GetNormal();
        public RaycastSensor GetRaycastSensor() => _raycastSensor;
        public void SetVelocity(Vector3 velocity) => _rb.linearVelocity = velocity + _currentGroundAdjustmentVelocity;
        public void SetSensorRange(float amount) => _adjustedSensorRange = _baseSensorRange * amount;
        public Vector3 GetRigidbodyVelocity() => _rb.linearVelocity;
        public Quaternion GetRigidbodyRotation() => _rb.rotation;
        public void SetRigidbodyRotation(Quaternion rotation) => _rb.MoveRotation(rotation);
        public void ApplyForce(Vector3 direction) => _rb.AddForce(direction, horizontalForceMode);
        public void SetLinearDampening(float amount) => _rb.linearDamping = amount;
        
        private void Setup() {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            
            _rb.freezeRotation = true;
            _rb.useGravity = true;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void RecalculateColliderDimensions() {
            if (_collider ==null) 
                Setup();

            _collider.height = colliderHeight * (1f - stepHeightRatio);
            _collider.radius = colliderDiameter * 0.5f;
            _collider.center = colliderOffset * 
                               colliderHeight + 
                               new Vector3(0f, stepHeightRatio * _collider.height * 0.5f, 0f);
            
            if (_collider.height * 0.5f < _collider.radius)
                _collider.radius = _collider.height * 0.5f;

            RecalibrateSensor();
        }

        private void RecalibrateSensor() {
            _raycastSensor ??= new RaycastSensor(_tr);
            
            _raycastSensor.SetCastOrigin(_collider.bounds.center);
            _raycastSensor.SetCastDirection(Helper.CastDirection.Down);

            RecalculateSensorLayerMask();

            // Prevent clipping issues when the sensor range is calculated.
            const float offsetProtrusion = 0.001f;
            
            var length = colliderHeight * (1f - stepHeightRatio) * 0.5f + colliderHeight * stepHeightRatio;
            _baseSensorRange = length * (1f + offsetProtrusion) * _tr.localScale.x;
            _raycastSensor.CastLength = length * _tr.localScale.x;
        }

        private void RecalculateSensorLayerMask() {
            var objLayer = gameObject.layer;
            var layerMask = Physics.AllLayers;

            for (var i = 0; i < 32; i++)
                if (Physics.GetIgnoreLayerCollision(objLayer, i))
                    layerMask &= ~(1 << i);
            
            var ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);
            
            _raycastSensor.LayerMask = layerMask;
            _currentLayer = objLayer;
        }

        public void ApplyJumpForce(float jumpForce) {
            _rb.AddForce(_tr.up * jumpForce, verticalForceMode);
        }
        
        /// <summary>
        /// Unused and experimental.
        /// </summary>
        private int _maxBounces = 5;
        private float _skinWidth = 0.005f;
        private float _maxSlopeAngle = 55f;
        private Vector3 CollideAndSlide(Vector3 velocity, Vector3 pos, int depth, bool gravityPass, Vector3 initialVelocity) {
            var bounds = _collider.bounds;
            bounds.Expand(-2 * _skinWidth);
            
            if (depth >= _maxBounces) {
                return Vector3.zero;
            }
            
            var dist = velocity.magnitude + _skinWidth;

            if (!Physics.SphereCast(_collider.center, _collider.radius, velocity.normalized, out var hit,
                        dist, 1 << 6)) 
                return velocity;

            var snapToSurface = velocity.normalized * (hit.distance - _skinWidth);
            var remaining = velocity - snapToSurface;
            var angle = Vector3.Angle(Vector3.up, hit.normal);

            if (snapToSurface.magnitude <= _skinWidth)
                snapToSurface = Vector3.zero;

            if (angle <= _maxSlopeAngle) {
                if (gravityPass) {
                    return snapToSurface;
                }

                remaining = ProjectAndScale(remaining, hit.normal);
            }
            else {
                var scale = 1 - Vector3.Dot(
                        new Vector3(hit.normal.x, 0, hit.normal.z).normalized,
                        -new Vector3(initialVelocity.x, 0, initialVelocity.z).normalized);
                
                if (_isGrounded && !gravityPass) {
                    remaining = ProjectAndScale(
                            new Vector3(remaining.x, 0, remaining.z),
                            new Vector3(hit.normal.x, 0, hit.normal.z)
                            ).normalized;
                    remaining *= scale;
                }
                else {
                    remaining = ProjectAndScale(remaining, hit.normal) * scale;
                }
            }
            
            return snapToSurface + 
                   CollideAndSlide(
                           remaining, 
                           pos + snapToSurface, 
                           depth + 1, 
                           gravityPass, 
                           initialVelocity);
        }

        private static Vector3 ProjectAndScale(Vector3 vec, Vector3 normal) {
            var mag = vec.magnitude;
            vec = Vector3.ProjectOnPlane(vec, normal).normalized;
            vec *= mag;
            return vec;
        }
    }
}