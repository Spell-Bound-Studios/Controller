using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    public class RaycastSensor {
        public float CastLength = 1f;
        public LayerMask LayerMask = 1 << 16;
        
        private Vector3 _origin = Vector3.zero;
        private Transform _tr;
        private RaycastHit _hit;
        private Helper.CastDirection _castDirection;

        public RaycastSensor(Transform playerTransform) => _tr = playerTransform;

        public void CastRaycast() {
            var worldOrigin = _tr.TransformPoint(_origin);
            var worldDir = GetCastDirection();
            
            Physics.Raycast(worldOrigin, worldDir, out _hit, CastLength, LayerMask, QueryTriggerInteraction.Ignore);
        }
        
        public bool HasDetectedHit() => _hit.collider != null;
        public float GetDistance() => _hit.distance;
        public Vector3 GetNormal() => _hit.normal;
        public Vector3 GetPosition() => _hit.point;
        public Collider GetCollider() => _hit.collider;
        public Transform GetTransform() => _hit.transform;
        public void SetCastOrigin(Vector3 pos) => _origin = _tr.InverseTransformPoint(pos);
        public void SetCastDirection(Helper.CastDirection direction) => _castDirection = direction;

        private Vector3 GetCastDirection() {
            return _castDirection switch {
                    Helper.CastDirection.Forward => _tr.forward,
                    Helper.CastDirection.Backward => -_tr.forward,
                    Helper.CastDirection.Left => -_tr.right,
                    Helper.CastDirection.Right => _tr.right,
                    Helper.CastDirection.Up => _tr.up,
                    Helper.CastDirection.Down => -_tr.up,
                    _ => Vector3.one
            };
        }
    }
}