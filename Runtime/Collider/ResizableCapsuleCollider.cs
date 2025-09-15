using System;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ResizableCapsuleCollider {
        public CapsuleColliderData CapsuleColliderData { get; private set; }
        [field: SerializeField] public DefaultColliderData DefaultColliderData { get; private set; }
        [field: SerializeField] public SlopeData SlopeData { get; private set; }
        
        public void Initialize(GameObject go) {
            if (CapsuleColliderData != null)
                return;

            CapsuleColliderData = new CapsuleColliderData();
            CapsuleColliderData.Initialize(go);
        }
        
        /// <summary>
        /// Recalculates capsule dimensions based on current settings.
        /// Call this when you need to update the collider size.
        /// </summary>
        public void CalculateCapsuleColliderDimensions() {
            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight(DefaultColliderData.Height * (1f - SlopeData.StepHeightPercentage));

            RecalculateCapsuleColliderCenter();
            RecalculateColliderRadius();
            
            CapsuleColliderData.UpdateColliderData();
        }
        
        /// <summary>
        /// This is the new capsule that has been "stepped up". Therefore, its center should...  
        /// </summary>
        public void RecalculateCapsuleColliderCenter() {
            // Old collider vs. new height. (2 - 1.5) = 0.5
            var colliderHeightDiff = DefaultColliderData.Height - CapsuleColliderData.Collider.height;
            // Centers are local... So this should simply shift in the direction of the difference by half.
            var newColliderCenter = new Vector3(0f, DefaultColliderData.CenterY + colliderHeightDiff * 0.5f, 0f);
            CapsuleColliderData.Collider.center = newColliderCenter;
        }
        
        public void RecalculateColliderRadius() {
            var halfColliderHeight = CapsuleColliderData.Collider.height * 0.5f;

            if (halfColliderHeight >= CapsuleColliderData.Collider.radius)
                return;

            SetCapsuleColliderRadius(halfColliderHeight);
        }
        
        public void SetCapsuleColliderRadius(float r) => CapsuleColliderData.Collider.radius = r;
        public void SetCapsuleColliderHeight(float h) => CapsuleColliderData.Collider.height = h;
        
        public float CalculateTargetFloatingDistance(Transform transform) {
            return CapsuleColliderData.ColliderCenterInLocalSpace.y * transform.localScale.y;
        }
    }
}