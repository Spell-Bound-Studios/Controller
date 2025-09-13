using SpellBound.Controller.ManagersAndStatics;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    public abstract class BaseLocoStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        protected float HSpeedModifier = 1f;
        
        protected override void OnCtxInitialized() {
            Ctx = base.Ctx as PlayerControllerExample;
        }

        protected virtual void HandleInput() {
            var inputDesired = GetInputDirectionRelativeToCamera();
        
            var horizontalVelocity = 
                    Ctx.StatData.slopeSpeedModifier * HSpeedModifier * Ctx.StatData.movementSpeed * inputDesired - 
                    GetHorizontalVelocity();
            
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
        /// This is just an example. Personally, I would put something like this in a lower level of abstraction or
        /// access because I think that all loco states should likely be checking for ground.
        /// </summary>
        protected virtual void PerformGroundCheck() {
            Debug.Log("Checking ground");
            var colliderOriginInWorldSpace = 
                    Ctx.ResizableCapsuleCollider.CapsuleColliderData.Collider.bounds.center;
            var rayDistance = Ctx.ResizableCapsuleCollider.SlopeData.RayDistance;

            if (!ControllerHelper.CheckGroundRaycast(
                        colliderOriginInWorldSpace,
                        -Ctx.planarUp,
                        rayDistance,
                        Ctx.LayerData.GroundLayer,
                        out var hit
                )) {
                Ctx.StateData.Grounded = false;
                return;
            }
            
            Ctx.StateData.Grounded = true;
                
            var distanceToGround =
                    Ctx.ResizableCapsuleCollider.CapsuleColliderData.ColliderCenterInLocalSpace.y * 
                    Ctx.gameObject.transform.localScale.y - hit.distance;
            
            // Base case and should rarely happen.
            if (distanceToGround == 0)
                return;

            var liftDistance = distanceToGround * Ctx.ResizableCapsuleCollider.SlopeData.StepReachForce -
                               GetVerticalSpeed();

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
        
        protected virtual Vector3 GetHorizontalVelocity() => ControllerHelper.GetHorizontalVelocity(Ctx.Rb);
        
        protected virtual float GetVerticalSpeed() => ControllerHelper.GetVerticalSpeed(Ctx.Rb, Ctx.planarUp);
    }
}