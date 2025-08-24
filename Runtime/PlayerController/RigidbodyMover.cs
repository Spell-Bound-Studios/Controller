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

        [Header("Sensor Settings:")]
        [SerializeField] private bool isDebugging;
        private bool _isUsingExtendedSensorRange;

        private Transform _tr;
        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private RaycastSensor _raycastSensor;

        private bool _isGrounded;
        private float _baseSensorRange;
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
            _raycastSensor.CastLength = _isUsingExtendedSensorRange
                    ? _baseSensorRange + colliderHeight * _tr.localScale.x * stepHeightRatio
                    : _baseSensorRange;
            
            _raycastSensor.SphereRadius = _collider.radius;
            _raycastSensor.SphereCastLength = _colliderHalfSize + stepHeightRatio;
            
            _raycastSensor.CastRaycast();

            _isGrounded = _raycastSensor.HasDetectedHit();

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
        public Vector3 GetGroundNormal() => _raycastSensor.GetNormal();
        public void SetVelocity(Vector3 velocity) => _rb.linearVelocity = velocity + _currentGroundAdjustmentVelocity;
        public void SetExtendSensorRange(bool isExtended) => _isUsingExtendedSensorRange = isExtended;
        public Vector3 GetRigidbodyVelocity() => _rb.linearVelocity;
        
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
            _rb.AddForce(_tr.up * jumpForce, ForceMode.Impulse);
        }
        
        private void OnDrawGizmosSelected() {
            if (!isDebugging)
                return;
            
            if (_raycastSensor == null) 
                return;

            var origin = _raycastSensor.GetCastOriginWorld();
            var dir = _raycastSensor.GetCastDirectionWorld();

            var rayLen = _raycastSensor.HasDetectedHit() 
                    ? _raycastSensor.GetRaycastHitDistance() 
                    : _raycastSensor.CastLength;
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(origin, origin + dir * rayLen);
            Gizmos.DrawSphere(origin + dir * rayLen, 0.06f);

            var sphereLen = _raycastSensor.HasDetectedSphereHit() 
                    ? _raycastSensor.GetSphereHitDistance() 
                    : _raycastSensor.SphereCastLength;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + dir * sphereLen);
            Gizmos.DrawWireSphere(origin + dir * sphereLen, _raycastSensor.SphereRadius);
        }
    }
}