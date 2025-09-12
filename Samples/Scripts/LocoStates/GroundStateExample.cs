using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public class GroundStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;

        protected float HSpeedModifier = 1f;
        protected RaycastHit Hit;
        
        protected override void OnCtxInitialized() {
            Ctx = base.Ctx as PlayerControllerExample;
        }
        
        protected override void EnterStateLogic() {
            Ctx.input.OnInteractPressed += HandleInteractPressed;
            Ctx.input.OnJumpInput += HandleJumpPressed;
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.StateData.Grounded)
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Falling);
        }

        protected override void FixedUpdateStateLogic() {
            GroundCheck();
            HandleInput();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() {
            Ctx.input.OnInteractPressed -= HandleInteractPressed;
            Ctx.input.OnJumpInput -= HandleJumpPressed;
        }

        protected virtual void HandleInput() {
            var inputDesired = GetInputDirectionRelativeToCamera();
        
            var horizontalVelocity = 
                    Ctx.StatData.slopeSpeedModifier * HSpeedModifier * Ctx.StatData.movementSpeed * inputDesired - 
                    GetHorizontalVelocity();
            
            Ctx.Rb.AddForce(horizontalVelocity, Ctx.RigidbodyData.horizontalForceMode);
        }
        
        protected virtual Vector3 GetInputDirectionRelativeToCamera() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                            Ctx.referenceTransform.right, Ctx.planarUp).normalized * 
                    Ctx.input.Direction.x + 
                    Vector3.ProjectOnPlane(
                            Ctx.referenceTransform.forward, Ctx.planarUp).normalized * 
                    Ctx.input.Direction.y;
            
            return direction.magnitude > 1f 
                    ? direction.normalized 
                    : direction;
        }
        
        protected Vector3 GetHorizontalVelocity() {
            var v = Ctx.Rb.linearVelocity;
            v.y = 0;
            return v;
        }
        
        protected float GetVerticalSpeed() => Vector3.Dot(Ctx.Rb.linearVelocity, Ctx.planarUp);
        
        /// <summary>
        /// This is just an example. Personally, I would put something like this in a lower level of abstraction or
        /// access because I think that all loco states should likely be checking for ground.
        /// </summary>
        private void GroundCheck() {
            var colliderOriginInWorldSpace = 
                    Ctx.ResizableCapsuleCollider.CapsuleColliderData.Collider.bounds.center;

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
                    Ctx.ResizableCapsuleCollider.CapsuleColliderData.ColliderCenterInLocalSpace.y * 
                    Ctx.gameObject.transform.localScale.y - Hit.distance;
            
            // Base case and should rarely happen.
            if (distanceToGround == 0)
                return;

            var liftDistance = distanceToGround * Ctx.ResizableCapsuleCollider.SlopeData.StepReachForce -
                               GetVerticalSpeed();

            var liftForce = new Vector3(0f, liftDistance, 0f);
                
            Ctx.Rb.AddForce(liftForce, Ctx.RigidbodyData.horizontalForceMode);
        }
        
        protected virtual void HandleCharacterRotation() {
            var planarVelocity = Vector3.ProjectOnPlane(
                    Ctx.Rb.linearVelocity, Ctx.planarUp);

            if (planarVelocity.sqrMagnitude < 1e-6f)
                return;

            var desiredDir = planarVelocity.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDir, Ctx.planarUp);
            var angleDiff = Quaternion.Angle(Ctx.Rb.rotation, targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, Ctx.RotationData.RotationFallOffAngle, angleDiff);
            
            var maxStepDeg = Ctx.RotationData.turnTowardsInputSpeed * speedFactor * Time.fixedDeltaTime;

            var nextRotation = Quaternion.RotateTowards(Ctx.Rb.rotation, targetRotation, maxStepDeg);
            
            Ctx.Rb.MoveRotation(nextRotation);
        }

        protected virtual void HandleInteractPressed() {
            Ctx.locoStateMachine.ChangeVariant(LocoStateTypes.Grounded, Ctx.locoStates[1]);
        }
        
        private void HandleJumpPressed() {
            Ctx.locoStateMachine.ChangeState(LocoStateTypes.Jumping);
        }
    }
}