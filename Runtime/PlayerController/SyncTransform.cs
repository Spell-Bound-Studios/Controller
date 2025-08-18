using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Place this on a gameobject that you want to 1:1 match the transform with some offset.
    /// </summary>
    public class SyncTransform : MonoBehaviour {
        [SerializeField] private Transform followThisTransform;
        [SerializeField] private Vector3 offset = new(0f, 2f, 0f);
        private Transform _tr;
        
        private void Awake() {
            _tr = transform;
            
            if (!followThisTransform)
                Debug.LogError("trackThisTransform is not set. Please drag it in via inspector.", this);
        }
        
        private void LateUpdate() {
            _tr.position = followThisTransform.position + offset;
        }
    }
}