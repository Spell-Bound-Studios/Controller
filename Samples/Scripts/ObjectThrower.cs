using System.Collections.Generic;
using UnityEngine;
using SpellBound.Controller.PlayerInputs;

namespace SpellBound.Controller.Samples {
    public class ObjectThrower : MonoBehaviour {
        [SerializeField] private PlayerInputActionsSO playerInputActionsSO;
        [SerializeField] private float throwSpeed = 15f;
        [SerializeField] private float timeUntilDestroy = 10f;
        [SerializeField] private float spawnForwardOffset = 0.5f;
        private readonly List<GameObject> _templates = new();
        private int _nextIndex;
        
        private void Awake() {
            if (!playerInputActionsSO)
                Debug.LogError("ObjectThrow is missing playerInputActionsSO. Please drag and drop it.", this);
            
            CollectChildTemplates();
        }

        private void OnEnable() {
            if (playerInputActionsSO != null)
                playerInputActionsSO.OnInteractPressed += HandleEPressed;
        }

        private void OnDisable() {
            if (playerInputActionsSO != null)
                playerInputActionsSO.OnInteractPressed -= HandleEPressed;
        }

        private void CollectChildTemplates() {
            _templates.Clear();
            
            foreach (Transform child in transform) {
                if (child == null || !child.gameObject.activeSelf)
                    continue;
                
                _templates.Add(child.gameObject);
            }

            if (_templates.Count == 0)
                Debug.LogWarning("PlaneObjectThrower: No child templates found. Add children under this object to throw.", this);
        }

        private void HandleEPressed() {
            if (_templates.Count == 0) 
                return;

            var template = _templates[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _templates.Count;

            LaunchTemplate(template);
        }
        
        private void LaunchTemplate(GameObject template) {
            if (template == null) 
                return;

            var dir = transform.up.normalized;
            var spawnPos = transform.position + dir * spawnForwardOffset;
            var rotation = Quaternion.LookRotation(dir, Vector3.up);

            var go = Instantiate(template, spawnPos, rotation);
            
            if (!go.TryGetComponent<Rigidbody>(out var rb)) {
                rb = go.AddComponent<Rigidbody>();
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.interpolation = RigidbodyInterpolation.Interpolate;
                Debug.LogWarning("PlaneObjectThrower: Template had no Rigidbody; added one at runtime.", go);
            }
            
            rb.AddForce(dir * throwSpeed, ForceMode.VelocityChange);
            
            Destroy(go, timeUntilDestroy);
        }
    }
}