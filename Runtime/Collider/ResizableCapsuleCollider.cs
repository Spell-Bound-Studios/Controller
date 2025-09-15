using System;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ResizableCapsuleCollider {
        // This is the literal capsule that is being modified.
        public CapsuleColliderData CapsuleColliderData { get; private set; }
        // This is the "old" capsule that is not being modified. It's essentially the player visual.
        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; } = new();
        // This is where step height percentage, raycasts, and force data will live.
        [field: SerializeField] public SlopeData SlopeData { get; private set; }
        // This is where floating info will live.
        [field: SerializeField] public CapsuleFloatData CapsuleFloatData { get; private set; }
        
        public void Initialize(GameObject go) {
            if (CapsuleColliderData != null)
                return;
            
            CapsuleColliderData = new CapsuleColliderData();
            CapsuleColliderData.Initialize(go);
            
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
            CapsuleColliderData.Collider.center = 
                    new Vector3(0f, DefaultColliderData.Height * (1f + SlopeData.StepHeightPercentage) * 0.5f, 0f);
            
            Debug.Log($"CapsuleCollider Center: {CapsuleColliderData.Collider.center}");
            Debug.Log($"Default Collider Height: {DefaultColliderData.Height}");
            Debug.Log($"StepHeightPercentage: {SlopeData.StepHeightPercentage}");
            
            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight((DefaultColliderData.Height - CapsuleColliderData.Collider.center.y) * 2f);
            Debug.Log("...");
            Debug.Log($"Default Collider Height: {DefaultColliderData.Height}");
            Debug.Log($"CapsuleCollider Center.y: {CapsuleColliderData.Collider.center.y}");
            Debug.Log("...");
        }
        
        public void SetCapsuleColliderRadius(float r) => CapsuleColliderData.Collider.radius = r;
        public void SetCapsuleColliderHeight(float h) => CapsuleColliderData.Collider.height = h;
        
        public float CalculateTargetFloatingDistance(Transform transform) {
            return CapsuleColliderData.ColliderCenterInLocalSpace.y * transform.localScale.y;
        }
    }
}