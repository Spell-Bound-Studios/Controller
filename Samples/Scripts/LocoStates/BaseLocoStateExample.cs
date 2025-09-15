using UnityEngine;

namespace SpellBound.Controller.Samples {
    public abstract class BaseLocoStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        protected float HSpeedModifier = 1f;
        
        // All inheritors will have access to this Ctx.
        protected override void OnCtxInitialized() {
            Ctx = base.Ctx as PlayerControllerExample;
        }
        
        // All inheritors will automatically run this FixedUpdateStateLogic unless they override it.
        protected override void FixedUpdateStateLogic() {
            PerformGroundCheck();
            HandleInput();
            HandleCharacterRotation();
        }

        protected virtual void HandleInput() {
            var inputDesired = GetInputDirectionRelativeToCamera();
        
            var horizontalVelocity = 
                    Ctx.StatData.slopeSpeedModifier * HSpeedModifier * Ctx.StatData.movementSpeed * inputDesired - 
                    ControllerHelper.GetHorizontalVelocity(Ctx.Rb);
            
            Ctx.Rb.AddForce(horizontalVelocity, Ctx.RigidbodyData.horizontalForceMode);
        }
        
        protected virtual Vector3 GetInputDirectionRelativeToCamera() {
            return ControllerHelper.GetInputDirectionRelativeToCamera(
                    Ctx.input.Direction,
                    Ctx.referenceTransform,
                    Ctx.planarUp
            );
        }
        
        /// <summary>
        /// This method checks for ground and controls the Ctx.StateData.Grounded bool by flipping it if it hits the
        /// ground with its raycast. It also provides a lift force to enable stepping up or down.
        /// <remarks>
        /// This is just an example. But we wanted to show how you could put this in a lower level of abstraction, or
        /// we think that this is potentially something that all loco states could use.
        /// </remarks>
        /// </summary>
        protected virtual void PerformGroundCheck() {
            // This is the center of the capsule in world space.
            var rayOrigin = Ctx.ResizableCapsuleCollider.CapsuleColliderData.Collider.bounds.center;
            var rayDistance = Ctx.ResizableCapsuleCollider.SlopeData.RayDistance;
            var upDirection = Ctx.planarUp;
            
            
            if (!Physics.Raycast(
                        origin: rayOrigin,
                        direction: -upDirection,
                        hitInfo: out var hit,
                        maxDistance: rayDistance,
                        layerMask: Ctx.LayerData.GroundLayer,
                        queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {
                Ctx.StateData.Grounded = false;
                return;
            }
            
            Ctx.StateData.Grounded = true;
            
            var distanceToGround = 
                    Ctx.ResizableCapsuleCollider.CapsuleColliderData.ColliderCenterInLocalSpace.y * Ctx.gameObject.transform.localScale.y -
                                   hit.distance;
            
            // Base case and should rarely happen.
            if (distanceToGround == 0)
                return;

            var liftDistance = distanceToGround * Ctx.ResizableCapsuleCollider.SlopeData.StepReachForce -
                               ControllerHelper.GetVerticalSpeed(Ctx.Rb, Ctx.planarUp);

            var liftForce = new Vector3(0f, liftDistance, 0f);
                
            Ctx.Rb.AddForce(liftForce, Ctx.RigidbodyData.horizontalForceMode);
        }
        
        protected virtual void HandleCharacterRotation() {
            ControllerHelper.HandleCharacterRotation(
                    Ctx.Rb, 
                    Ctx.planarUp, 
                    Ctx.RotationData.turnTowardsInputSpeed, 
                    Ctx.RotationData.RotationFallOffAngle, 
                    Time.fixedDeltaTime);
        }
    }
}