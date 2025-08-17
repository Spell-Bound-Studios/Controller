using UnityEngine;

namespace SpellBound.Controller.Configuration {
    /// <summary>
    /// Place this on a gameobject that you want to 1:1 match the transform with some offset.
    /// </summary>
    public class CameraAnchorFollower : MonoBehaviour {
        [SerializeField] private Transform trackThisTransform;
        [SerializeField] private Vector3 offset = new(-0.2f, 1.5f, 0f);

        private void Awake() {
            if (!trackThisTransform)
                Debug.LogError("trackThisTransform is not set. Please drag it in via inspector.", this);
        }
        
        private void LateUpdate() {
            transform.position = trackThisTransform.position + offset;
        }
    }
}