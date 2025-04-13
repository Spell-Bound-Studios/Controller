using UnityEngine;

namespace SpellBound.CharacterController {
    public class RigidbodyHandler {
        private Rigidbody _rigidbody;
        
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
    }
}