using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public class GroundStateExample : BaseSoState {
        private PlayerControllerExample _ctx;

        protected override void OnCtxInitialized() {
            _ctx = Ctx as PlayerControllerExample;
        }
        
        protected override void EnterStateLogic() {

        }

        protected override void UpdateStateLogic() {
            Debug.Log("HELLO WORLD");
        }

        protected override void FixedUpdateStateLogic() {
            
        }

        protected override void ExitStateLogic() {
            
        }
        
        private void GroundCheck() {
            /*var colliderOriginInWorldSpace = Ctx.ResizableCapsuleCollider.CapsuleColliderData.Collider.bounds.center;

            if (!Physics.Raycast(
                        origin: colliderOriginInWorldSpace,
                        direction: -Ctx.planarUp,
                        hitInfo: out Hit,
                        maxDistance: Ctx.ResizableCapsuleCollider.SlopeData.RayDistance,
                        layerMask: Ctx.LayerData.GroundLayer,
                        queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {
                Ctx.StateData.Grounded = false;
                return;
            }
            
            Ctx.StateData.Grounded = true;
                
            var distanceToGround =
                    Ctx.ResizableCapsuleCollider.CapsuleColliderData.ColliderCenterInLocalSpace.y * Cc.gameObject.transform.localScale.y -
                    Hit.distance;
            
            // Base case and should rarely happen.
            if (distanceToGround == 0)
                return;

            var liftDistance = distanceToGround * Ctx.ResizableCapsuleCollider.SlopeData.StepReachForce -
                               GetVerticalSpeed();

            var liftForce = new Vector3(0f, liftDistance, 0f);
                
            Ctx.Rb.AddForce(liftForce, Ctx.RigidbodyData.horizontalForceMode);*/
        }
    }
}