using System;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
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
        
        public void CalculateCapsuleColliderDimensions() {
            SetCapsuleColliderRadius(DefaultColliderData.Radius);
            SetCapsuleColliderHeight(DefaultColliderData.Height * (1f - SlopeData.StepHeightPercentage));
            RecalculateCapsuleColliderCenter();
            RecalculateColliderRadius();
            CapsuleColliderData.UpdateColliderData();
        }

        public void SetCapsuleColliderRadius(float r) => CapsuleColliderData.Collider.radius = r;
        public void SetCapsuleColliderHeight(float h) => CapsuleColliderData.Collider.height = h;
        
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
    }
}