using UnityEngine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// POCO class intended to perform a raycast in a direction based on a transform given in the constructor.
    /// </summary>
    public class RaycastSensor {
        public float CastLength = 1f;
        public float SphereCastLength = 1f;
        public float SphereRadius = 1f;
        public LayerMask LayerMask = 1 << 6;
        
        private Vector3 _origin = Vector3.zero;
        private readonly Transform _tr;
        private RaycastHit _hit;
        private RaycastHit _sphereHit;
        private Helper.CastDirection _castDirection;

        public RaycastSensor(Transform playerTransform) => _tr = playerTransform;

        public void CastRaycast() {
            var worldOrigin = GetCastOriginWorld();
            var worldDir = GetCastDirection();

            Physics.Raycast(
                    origin: worldOrigin, 
                    direction: worldDir, 
                    hitInfo: out _hit, 
                    maxDistance: CastLength, 
                    layerMask: LayerMask,
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore);
            
            Physics.SphereCast(
                    origin: worldOrigin, 
                    radius: SphereRadius, 
                    direction: worldDir, 
                    hitInfo: out _sphereHit, 
                    maxDistance: SphereCastLength, 
                    layerMask: LayerMask, 
                    queryTriggerInteraction: QueryTriggerInteraction.Ignore);
        }
        
        public bool HasDetectedHit() => _hit.collider != null;
        public float GetRaycastHitDistance() => _hit.distance;
        public Vector3 GetNormal() => _hit.normal;
        public Vector3 GetPosition() => _hit.point;
        public Collider GetCollider() => _hit.collider;
        public Transform GetTransform() => _hit.transform;
        public void SetCastOrigin(Vector3 pos) => _origin = _tr.InverseTransformPoint(pos);
        public void SetCastDirection(Helper.CastDirection direction) => _castDirection = direction;
        public bool HasDetectedSphereHit() => _sphereHit.collider !=null;
        public float GetSphereHitDistance() => _sphereHit.distance;
        public Vector3 GetSphereHitPoint() => _sphereHit.point;
        public Vector3 GetCastOriginWorld() => _tr.TransformPoint(_origin);
        public Vector3 GetCastDirectionWorld() => GetCastDirection();

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