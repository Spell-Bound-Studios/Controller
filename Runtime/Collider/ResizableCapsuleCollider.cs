using System;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// WARNING! Please ensure that your player visual, wherever it might be, has its feet at y=0. If you do not do this
    /// your collider and visual be mismatched.
    /// </summary>
    [Serializable]
    public class ResizableCapsuleCollider {
        public CapsuleCollider collider;
        // This is the "old" capsule that is not being modified. It's essentially the player visual.
        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; } = new();
        // This is where step height percentage, raycasts, and force data will live.
        [field: SerializeField] public SlopeData SlopeData { get; private set; }
        // This is where floating info will live.
        [field: SerializeField] public CapsuleFloatData CapsuleFloatData { get; private set; }
        
        public void Initialize(GameObject go) {
            if (collider != null)
                return;
            
            collider = go.AddComponent<CapsuleCollider>();
            
            // This will calculate all the base default data.
            DefaultColliderData.Initialize(go);
        }
        
        /// <summary>
        /// Recalculates capsule dimensions based on current settings - keep separate from Initialize to avoid rewriting
        /// objects at runtime.
        /// Call this when you need to update the collider size.
        /// </summary>
        public void CalculateCapsuleColliderDimensions() {
            // The first thing it should do is calculate the center.
            collider.center = 
                    new Vector3(0f, DefaultColliderData.Height * (1f + SlopeData.StepHeightPercentage) * 0.5f, 0f);
            
            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight((DefaultColliderData.Height - collider.center.y) * 2f);

        }
        
        public void SetCapsuleColliderRadius(float r) => collider.radius = r;
        public void SetCapsuleColliderHeight(float h) => collider.height = h;
        
        public float CalculateTargetFloatingDistance(Transform transform) {
            return collider.center.y * transform.localScale.y;
        }
    }
}