using System;
using UnityEngine;

namespace SpellBound.Controller {
    [Serializable]
    public class DefaultColliderData {
        [Header("Collider Configuration")]
        [field: SerializeField] public float Height { get; private set; }
        [field: SerializeField] public float CenterY { get; private set; }
        [field: SerializeField] public float Radius { get; private set; }
        
        // If the user doesn't want to automatically calculate based on mesh bounds then set to true.
        [Header("Override Settings")]
        [SerializeField] private bool overrideHeight;
        [SerializeField] private bool overrideCenter;
        [SerializeField] private bool overrideRadius;

        public bool OverrideHeight => overrideHeight;
        public bool OverrideCenter => overrideCenter;
        public bool OverrideRadius => overrideRadius;
        
        [Header("Fallback Values")]
        [field: SerializeField, Range(0.1f, 10f)] public float FallbackHeight { get; private set; } = 1.8f;
        [field: SerializeField, Range(0.05f, 2f)] public float FallbackRadius { get; private set; } = 0.3f;
        
        [field: SerializeField] public bool SuppressWarnings { get; private set; }
        [field: SerializeField] public bool SuppressConsole { get; private set; } = true;
        
        public void Initialize(GameObject go) {
            Bounds bounds = default;
            var foundMesh = false;
            var meshSource = "";
            
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null) {
                bounds = smr.sharedMesh.bounds;
                foundMesh = true;
                meshSource = "SkinnedMeshRenderer";
            }
            else {
                smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
                if (smr != null && smr.sharedMesh != null) {
                    bounds = smr.sharedMesh.bounds;
                    foundMesh = true;
                    meshSource = "SkinnedMeshRenderer (child)";
                }
                else {
                    var meshFilter = go.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null) {
                        bounds = meshFilter.sharedMesh.bounds;
                        foundMesh = true;
                        meshSource = "MeshFilter";
                    }
                    else {
                        meshFilter = go.GetComponentInChildren<MeshFilter>();
                        if (meshFilter != null && meshFilter.sharedMesh != null) {
                            bounds = meshFilter.sharedMesh.bounds;
                            foundMesh = true;
                            meshSource = "MeshFilter (child)";
                        }
                    }
                }
            }
            
            if (!foundMesh) {
                if (!SuppressConsole)
                    Debug.LogWarning($"No mesh found on {go.name} or its children. " +
                                     $"Using fallback values for non-overridden values.");
                
                SetFallbackValues();
                return;
            }
            
            if (!SuppressConsole)
                Debug.Log($"Using mesh bounds from {meshSource} on {go.name}");
            
            if (!OverrideHeight) {
                Height = bounds.size.y;
                ValidateAndClampHeight();
            }
            
            if (!OverrideCenter)
                CenterY = Height * 0.5f;
            
            if (!OverrideRadius) {
                var meshRadius = Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f;
                Radius = meshRadius;
                ValidateAndClampRadius();
            }

            ValidateConfiguration();
        }
        
        private void SetFallbackValues() {
            if (!OverrideHeight) {
                Height = FallbackHeight;
                ValidateAndClampHeight();
            }
            
            if (!OverrideCenter)
                CenterY = Height * 0.5f;

            if (!OverrideRadius) {
                Radius = FallbackRadius;
                ValidateAndClampRadius();
            }
        }
    
        private void ValidateAndClampHeight() {
            if (Height <= 0f) {
                Debug.LogWarning($"Calculated height {Height} is invalid. Using fallback height {FallbackHeight}.");
                Height = FallbackHeight;
            }
        
            // Reasonable limits for character height
            Height = Mathf.Clamp(Height, 0.1f, 10f);
        }
    
        private void ValidateAndClampRadius() {
            if (Radius <= 0f) {
                Debug.LogWarning($"Calculated radius {Radius} is invalid. Using fallback radius {FallbackRadius}.");
                Radius = FallbackRadius;
            }
            
            Radius = Mathf.Clamp(Radius, 0.05f, 200f);
        }
    
        private void ValidateConfiguration() {
            if (CenterY > Height) {
                Debug.LogWarning($"CenterY ({CenterY}) cannot be higher than Height ({Height}). Clamping to height.");
                CenterY = Height;
            }
            
            if (CenterY < 0f) {
                Debug.LogWarning($"CenterY ({CenterY}) cannot be negative. Setting to 0.");
                CenterY = 0f;
            }
        
            if (!SuppressConsole)
                Debug.Log($"DefaultColliderData initialized: Height={Height:F2}, CenterY={CenterY:F2}, Radius={Radius:F2}");
        }
    
        /// <summary>
        /// Call this when values are changed in inspector to re-validate
        /// </summary>
        public void HandleValidation() {
            if (!OverrideCenter && Height > 0)
                CenterY = Height * 0.5f;
            
            ValidateAndClampHeight();
            ValidateAndClampRadius();
            ValidateConfiguration();
        }
    }
}