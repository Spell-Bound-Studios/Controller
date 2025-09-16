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
            var rayOrigin = Ctx.ResizableCapsuleCollider.collider.bounds.center;
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
    
            var targetDistance = Ctx.ResizableCapsuleCollider.CalculateTargetFloatingDistance(Ctx.transform);
            var actualDistance = hit.distance;
    
            var distanceToFloatingPoint = targetDistance - actualDistance;
            
            if (Mathf.Approximately(distanceToFloatingPoint, 0f))
                return;

            var amountToLift = 
                    distanceToFloatingPoint * Ctx.ResizableCapsuleCollider.SlopeData.StepReachForce - Ctx.Rb.linearVelocity.y;
    
            if (!Mathf.Approximately(amountToLift, 0f)) {
                Ctx.Rb.AddForce(Vector3.up * amountToLift, ForceMode.VelocityChange);
            }
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