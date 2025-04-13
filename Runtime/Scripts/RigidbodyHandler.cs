using UnityEngine;

namespace SpellBound.CharacterController {
    public class RigidbodyHandler {
        // Innate
        private Rigidbody _rigidbody;
        
        // Attributes
        private readonly float _speed;
        private Vector3 _cameraInputDir;
        private Vector3 _compositeDir;
        
        /// <summary>
        /// Initialize your custom or default Rigidbody.
        /// </summary>
        public Rigidbody InitializeRigidbody(
            Rigidbody rigidbody = null,
            bool isKinematic = false,
            bool useGravity = true,
            float mass = 1.0f
        ) {
            if (rigidbody == null) {
                Debug.LogWarning("RigidbodyHandler: rigidbody is null");
                return null;
            }

            _rigidbody = rigidbody;
            _rigidbody.isKinematic = isKinematic;
            _rigidbody.useGravity = useGravity;
            _rigidbody.mass = mass;

            return _rigidbody;
        }
        
        /// <summary>
        /// Injects Camera.
        /// </summary>
        public void InjectCameraTransform() {
            
        }
        
        /// <summary>
        /// Injects Stats.
        /// </summary>
        public void InjectAttributes() {
            
        }
        
        /// <summary>
        /// Performs a ground check using a downward raycast.
        /// </summary>
        public bool GroundCheck(
            Transform groundCheckTransform,
            float checkDistance = 1.5f,
            LayerMask groundLayerMask = default,
            float originYOffset = 0.1f
            ) {
            var transform = groundCheckTransform ?? _rigidbody.transform;
            
            if (transform == null) {
                Debug.LogWarning("[RigidbodyHandler]: Requires a rigidbody or transform. Both were null.");
                return false;
            }

            var origin = transform.position + Vector3.up * originYOffset;
            var hit = UnityEngine.Physics.Raycast(origin, Vector3.down, checkDistance, groundLayerMask);

            return hit;
        }
        
        /// <summary>
        /// Rigidbody Movement.
        /// </summary>
        public void HandleMoveInput(Vector2 inputVector, Transform camTransform, float deltaTime) {
            var input = new Vector3(inputVector.x, 0f, inputVector.y);

            // Camera orientation/initialization.
            var camForward = camTransform.forward;
            var camRight = camTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // Inputs x Camera.
            _cameraInputDir = camForward * input.z + camRight * input.x;
            _compositeDir = _cameraInputDir * (_speed * deltaTime);
            
            _rigidbody.MovePosition(_rigidbody.position + _compositeDir);
        }
    }
}