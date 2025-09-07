using SpellBound.Controller.PlayerController;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public abstract class BaseLocoStateSO : ScriptableObject {
        protected LocoStateMachine StateMachine;
        protected SbCharacterControllerBase Cc => StateMachine?.CharController;
        public string uid;
        public string assetName;
        
        protected float HSpeedModifier = 1f;
        protected float VSpeedModifer = 1f;
        protected RaycastHit Hit;
        
#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on an asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var newName = name;
            
            if (assetPath == null) {
                uid = string.Empty;
                return;
            }

            if (assetName != newName) {
                assetName = newName;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            
            var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (string.IsNullOrEmpty(uid) || uid != assetGuid) {
                uid = assetGuid;
            }
        }
#endif
        
        /// <summary>
        /// This method is called when the state is first entered.
        /// </summary>
        public abstract void EnterStateLogic(LocoStateMachine stateMachine);

        /// <summary>
        /// This method is called every frame while the state is active.
        /// </summary>
        public abstract void UpdateStateLogic();

        /// <summary>
        /// This method is called every fixed frame rate frame while the state is active.
        /// </summary>
        public abstract void FixedUpdateStateLogic();

        /// <summary>
        /// This method checks if the state should transition to another state and
        /// is called in EnterStateLogic in the specific SO.
        /// Think of this as a "What rips the player out of X state".
        /// i.e. an interrupt, movement, etc.
        /// </summary>
        public abstract void CheckSwitchStateLogic();

        /// <summary>
        /// This method is called when the state is exited.
        /// </summary>
        public abstract void ExitStateLogic();

        protected virtual void HandleInput() {
            var inputDesired = GetInputDirectionRelativeToCamera();
        
            var horizontalVelocity = 
                    Cc.StatData.slopeSpeedModifier * HSpeedModifier * Cc.StatData.movementSpeed * inputDesired - 
                    GetHorizontalVelocity();
            
            Cc.Rb.AddForce(horizontalVelocity, Cc.RigidbodyData.horizontalForceMode);
        }

        protected virtual void HandleCharacterRotation() {
            var planarVelocity = Vector3.ProjectOnPlane(
                    Cc.Rb.linearVelocity, Cc.planarUp);

            if (planarVelocity.sqrMagnitude < 1e-6f)
                return;

            var desiredDir = planarVelocity.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDir, Cc.planarUp);
            var angleDiff = Quaternion.Angle(Cc.Rb.rotation, targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, Cc.RotationData.RotationFallOffAngle, angleDiff);
            
            var maxStepDeg = Cc.RotationData.turnTowardsInputSpeed * speedFactor * Time.fixedDeltaTime;

            var nextRotation = Quaternion.RotateTowards(Cc.Rb.rotation, targetRotation, maxStepDeg);
            
            Cc.Rb.MoveRotation(nextRotation);
        }
        protected abstract void HandleAnimation();
        
        /// <summary>
        /// Returns a horizontal movement vector from camera-relative input.
        /// </summary>
        protected virtual Vector3 GetInputDirectionRelativeToCamera() {
            // Reference transform right and forward projected on this transforms up normal plane to get a proper direction.
            var direction =
                    Vector3.ProjectOnPlane(
                            Cc.referenceTransform.right, Cc.planarUp).normalized * 
                    Cc.input.Direction.x + 
                    Vector3.ProjectOnPlane(
                            Cc.referenceTransform.forward, Cc.planarUp).normalized * 
                    Cc.input.Direction.y;
            
            return direction.magnitude > 1f 
                    ? direction.normalized 
                    : direction;
        }

        protected Vector3 GetHorizontalVelocity() {
            var v = Cc.Rb.linearVelocity;
            v.y = 0;
            return v;
        }
        
        protected float GetHorizontalSpeed() => Vector3.ProjectOnPlane(Cc.Rb.linearVelocity, Cc.planarUp).magnitude;

        protected Vector3 GetVerticalVelocity() {
            var v = Cc.Rb.linearVelocity;
            v.x = 0;
            v.z = 0;
            return v;
        }
        
        protected float GetVerticalSpeed() => Vector3.Dot(Cc.Rb.linearVelocity, Cc.planarUp);
        
        protected virtual void GroundCheck() {
            var colliderOriginInWorldSpace = Cc.ResizableCapsuleCollider.CapsuleColliderData.Collider.bounds.center;

            if (!Physics.Raycast(
                        origin: colliderOriginInWorldSpace,
                        direction: -Cc.planarUp,
                        hitInfo: out Hit,
                        maxDistance: Cc.ResizableCapsuleCollider.SlopeData.RayDistance,
                        layerMask: Cc.LayerData.GroundLayer,
                        queryTriggerInteraction: QueryTriggerInteraction.Ignore)) {
                Cc.StateData.Grounded = false;
                return;
            }
            
            Cc.StateData.Grounded = true;
                
            var distanceToGround =
                    Cc.ResizableCapsuleCollider.CapsuleColliderData.ColliderCenterInLocalSpace.y * Cc.gameObject.transform.localScale.y -
                    Hit.distance;
            
            // Base case and should rarely happen.
            if (distanceToGround == 0)
                return;

            var liftDistance = distanceToGround * Cc.ResizableCapsuleCollider.SlopeData.StepReachForce -
                               GetVerticalSpeed();

            var liftForce = new Vector3(0f, liftDistance, 0f);
                
            Cc.Rb.AddForce(liftForce, Cc.RigidbodyData.horizontalForceMode);
        }
        
        private void SetSlopeSpeedModifierOnAngle(float angle) {
            var slopeSpeedMod = Cc.StateData.SlopeSpeedCurve.Evaluate(angle);
            Cc.StatData.slopeSpeedModifier = slopeSpeedMod;
        }

        protected Vector3 GetWeightForceComponent() =>
                Vector3.ProjectOnPlane(Cc.Rb.mass * Cc.StatData.gravity * -Cc.planarUp, Hit.normal);
    }
}