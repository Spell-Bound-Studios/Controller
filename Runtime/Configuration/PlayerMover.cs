using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMover : MonoBehaviour {
        [Header("Collider Settings:")] 
        [SerializeField, Range(0f, 1f)] private float stepHeightRatio = 0.1f;
        [SerializeField] private float colliderHeight = 2f;
        [SerializeField] private float colliderThickness = 1f;
        [SerializeField] private Vector3 colliderOffset = new(0, .4f, 0);

        [Header("Sensor Settings:")] 
        [SerializeField] private bool isDebugging;
        private bool _isUsingExtendedSensorRange = true;
        
        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private RaycastSensor _raycastSensor;

        private bool _isGrounded;
        private float _baseSensorRange;
        private Vector3 _currentGroundAdjustmentVelocity;
        private int _currentLayer;

        private void Awake() {
            Setup();
            RecalculateColliderDimensions();
        }

        private void OnValidate() {
            if (gameObject.activeInHierarchy)
                RecalculateColliderDimensions();
        }

        public void CheckForGround() {
            if (_currentLayer != gameObject.layer)
                RecalculateSensorLayerMask();

            _currentGroundAdjustmentVelocity = Vector3.zero;
            
            _raycastSensor.CastLength = _isUsingExtendedSensorRange
                    ? _baseSensorRange + colliderHeight * transform.localScale.x * stepHeightRatio
                    : _baseSensorRange;
            _raycastSensor.CastRaycast();

            _isGrounded = _raycastSensor.HasDetectedHit();

            if (!_isGrounded)
                return;

            var distance = _raycastSensor.GetDistance();
            var upperLimit = colliderHeight * transform.localScale.x * (1f - stepHeightRatio) * 0.5f;
            var middle = upperLimit + colliderHeight * transform.localScale.x * stepHeightRatio;
            var distanceToGo = middle - distance;

            _currentGroundAdjustmentVelocity = transform.up * (distanceToGo / Time.fixedDeltaTime);
        }

        public bool IsGrounded() => _isGrounded;
        public Vector3 GetGroundNormal() => _raycastSensor.GetNormal();
        public void SetVelocity(Vector3 velocity) => _rb.linearVelocity = velocity + _currentGroundAdjustmentVelocity;
        public void SetExtendSensorRange(bool isExtended) => _isUsingExtendedSensorRange = isExtended;
        
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
            _collider.radius = colliderThickness * 0.5f;
            _collider.center = colliderOffset * 
                               colliderHeight + 
                               new Vector3(0f, stepHeightRatio * _collider.height * 0.5f, 0f);
            
            if (_collider.height * 0.5f < _collider.radius)
                _collider.radius = _collider.height * 0.5f;

            RecalibrateSensor();
        }

        private void RecalibrateSensor() {
            _raycastSensor ??= new RaycastSensor(transform);
            
            _raycastSensor.SetCastOrigin(_collider.bounds.center);
            _raycastSensor.SetCastDirection(Helper.CastDirection.Down);

            RecalculateSensorLayerMask();

            // Prevent clipping issues when the sensor range is calculated.
            const float safetyDistanceFactor = 0.001f;

            var length = colliderHeight * (1f - stepHeightRatio) * 0.5f * colliderHeight * stepHeightRatio;
            _baseSensorRange = length * (1f + safetyDistanceFactor) * transform.localScale.x;
            _raycastSensor.CastLength = length * transform.localScale.x;
        }

        private void RecalculateSensorLayerMask() {
            var objLayer = gameObject.layer;
            var layerMask = Physics.AllLayers;

            for (var i = 0; i < 32; i++) {
                if (Physics.GetIgnoreLayerCollision(objLayer, i))
                    layerMask &= ~(1 << i);
            }
            
            var ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            layerMask &= ~(1 << ignoreRaycastLayer);
            
            _raycastSensor.LayerMask = layerMask;
            _currentLayer = objLayer;
        }
    }
}