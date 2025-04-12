using UnityEngine;

namespace SpellBound.CharacterController {
    public class RigidbodyHandler {
        /// <summary>
        /// Initialize your custom or default Rigidbody.
        /// </summary>
        public Rigidbody InitializeRigidbody(
            Rigidbody rb = null,
            bool isKinematic = false,
            bool useGravity = true,
            float mass = 1.0f
        ) {
            if (rb == null) {
                Debug.LogWarning("RigidbodyHandler: rb is null");
                return null;
            }

            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity;
            rb.mass = mass;

            return rb;
        }
    }
}