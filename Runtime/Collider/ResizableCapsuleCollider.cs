using System;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ResizableCapsuleCollider {
        public CapsuleColliderData CapsuleColliderData { get; private set; }
        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; }
        [field: SerializeField] public SlopeData SlopeData { get; private set; }
        [field: SerializeField] public AutoResizeData AutoResizeData { get; private set; }
        
        private float _currentStepHeightOverride;
        private bool _isStepHeightOverridden;
        private Transform _tr;
        
        public void Initialize(GameObject go) {
            if (CapsuleColliderData != null)
                return;
            
            _tr = go.transform;
            
            CapsuleColliderData = new CapsuleColliderData();
            CapsuleColliderData.Initialize(go);

            CalculateCapsuleColliderDimensions();
        }
        
        /// <summary>
        /// Recalculates capsule dimensions based on current settings.
        /// Call this when you need to update the collider size.
        /// </summary>
        public void CalculateCapsuleColliderDimensions() {
            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            
            var stepHeightPercentage = _isStepHeightOverridden 
                    ? _currentStepHeightOverride 
                    : SlopeData.StepHeightPercentage;
            
            SetCapsuleColliderHeight(DefaultColliderData.Height * (1f - stepHeightPercentage));
            
            RecalculateCapsuleColliderCenter();
            
            RecalculateColliderRadius();
            
            CapsuleColliderData.UpdateColliderData();
        }
        
        public void RecalculateCapsuleColliderCenter() {
            var colliderHeightDiff = DefaultColliderData.Height - CapsuleColliderData.Collider.height;
            var newColliderCenter = new Vector3(0f, DefaultColliderData.CenterY + colliderHeightDiff * 0.5f, 0f);
            CapsuleColliderData.Collider.center = newColliderCenter;
        }
        
        public void RecalculateColliderRadius() {
            var halfColliderHeight = CapsuleColliderData.Collider.height * 0.5f;

            if (halfColliderHeight >= CapsuleColliderData.Collider.radius) {
                return;
            }

            SetCapsuleColliderRadius(halfColliderHeight);
        }
        
        public void SetCapsuleColliderRadius(float r) => CapsuleColliderData.Collider.radius = r;
        public void SetCapsuleColliderHeight(float h) => CapsuleColliderData.Collider.height = h;
        
        /// <summary>
        /// Temporarily overrides step height percentage.
        /// Automatically calls CalculateCapsuleColliderDimensions().
        /// </summary>
        public void SetStepHeightOverride(float percentage) {
            _currentStepHeightOverride = Mathf.Clamp01(percentage);
            _isStepHeightOverridden = true;
            CalculateCapsuleColliderDimensions();
        }
        
        /// <summary>
        /// Clears step height override and returns to normal behavior.
        /// Automatically calls CalculateCapsuleColliderDimensions().
        /// </summary>
        public void ClearStepHeightOverride() {
            _isStepHeightOverridden = false;
            CalculateCapsuleColliderDimensions();
        }
        
        /// <summary>
        /// Gets the combined bounds of all mesh renderers on this GameObject and its children.
        /// </summary>
        private Bounds GetMeshBounds() {
            var meshRenderers = _tr.GetComponentsInChildren<MeshRenderer>();
            var skinnedMeshRenderers = _tr.GetComponentsInChildren<SkinnedMeshRenderer>();
            
            if (meshRenderers.Length == 0 && skinnedMeshRenderers.Length == 0)
                return new Bounds();
            
            var combinedBounds = new Bounds();
            var boundsInitialized = false;
            
            // Check regular mesh renderers
            foreach (var renderer in meshRenderers) {
                if (!boundsInitialized) {
                    combinedBounds = renderer.bounds;
                    boundsInitialized = true;
                } 
                else
                    combinedBounds.Encapsulate(renderer.bounds);
            }
            
            // Check skinned mesh renderers
            foreach (var renderer in skinnedMeshRenderers) {
                if (!boundsInitialized) {
                    combinedBounds = renderer.bounds;
                    boundsInitialized = true;
                } 
                else
                    combinedBounds.Encapsulate(renderer.bounds);
            }
            
            // Convert to local space
            var localCenter = _tr.InverseTransformPoint(combinedBounds.center);
            var localSize = Vector3.Scale(combinedBounds.size, 
                    new Vector3(1f / _tr.lossyScale.x, 1f / _tr.lossyScale.y, 1f / _tr.lossyScale.z));
            
            return new Bounds(localCenter, localSize);
        }
        
        /// <summary>
        /// Auto-resizes the capsule collider to fit the mesh geometry if enabled.
        /// Call this after mesh changes or when you want to fit to the current mesh.
        /// </summary>
        public void AutoResizeToMesh() {
            if (!AutoResizeData.EnableAutoResize) return;
            
            var meshBounds = GetMeshBounds();
            if (meshBounds.size == Vector3.zero) return; // No mesh found
            
            // Set dimensions based on mesh bounds
            var meshHeight = meshBounds.size.y;
            var meshRadius = Mathf.Max(meshBounds.size.x, meshBounds.size.z) * 0.5f;
            
            // Apply scaling factors
            var scaledHeight = meshHeight * AutoResizeData.MeshHeightScale;
            var scaledRadius = meshRadius * AutoResizeData.MeshRadiusScale;
            
            // Update default data
            UpdateDefaultColliderData(scaledHeight, scaledRadius, meshBounds.center.y);
            
            // Apply step height reduction
            CalculateCapsuleColliderDimensions();
        }
        
        /// <summary>
        /// Updates the default collider data based on mesh dimensions.
        /// Uses reflection to update the private setters - you may want to add public setters instead.
        /// </summary>
        private void UpdateDefaultColliderData(float height, float radius, float centerY) {
            var defaultData = DefaultColliderData;
            
            // Update using reflection (not ideal but works with private setters)
            var heightField = typeof(DefaultColliderData).GetField("<Height>k__BackingField", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            heightField?.SetValue(defaultData, height);
            
            var radiusField = typeof(DefaultColliderData).GetField("<Radius>k__BackingField", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            radiusField?.SetValue(defaultData, radius);
                
            var centerField = typeof(DefaultColliderData).GetField("<CenterY>k__BackingField", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            centerField?.SetValue(defaultData, centerY);
        }
    }
}