using UnityEngine;

namespace SpellBound.Controller.Configuration {
    public class CameraAnchorFollower : MonoBehaviour {
        [SerializeField] private Transform playerRoot;
        [SerializeField] private Vector3 offset = new(-0.2f, 1.5f, 0f);

        private void Awake() {
            if (!playerRoot)
                Debug.LogError("PlayerRoot is not set. Please drag it in via inspector.", this);
        }
        
        private void LateUpdate() {
            transform.position = playerRoot.position + offset;
        }
    }
}