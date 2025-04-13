using SpellBound.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.CharacterController {
    /// <summary>
    /// Base Character Controller class to be inherited by a component class. This will do a lot of behind the scenes
    /// for the user. Alternatively, they will be given full control for every function if they want to change it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseCharacterController : MonoBehaviour {
        // Components
        protected Rigidbody Rigidbody;
        
        // Handlers
        protected RigidbodyHandler RbHandler;
        protected CharacterInputHandler InputHandler;
        
        // State Machine
        protected StateContext StateCtx;
        
        // Ground Checking
        [SerializeField] protected LayerMask groundLayer;
        protected float GroundCheckDistance;
        
        // Debugging Only
        [SerializeField] private bool debugging;
        [SerializeField] private BaseStateSO baseActionStateSO;
        [SerializeField] private BaseStateSO baseLocoStateSO;
        
        private void Awake() {
            // Debugging
            OnStateDebugging(awake: true);
            
            // Rigidbody
            Rigidbody = GetComponent<Rigidbody>();
            RbHandler = new RigidbodyHandler();
            Rigidbody = RbHandler.InitializeRigidbody(Rigidbody);
            
            // Inputs
            InputHandler = new CharacterInputHandler(this);
            
            // Create state context to inject into the state handlers.
            StateCtx = new StateContext();
            
            // Ground Checks
            GroundCheckDistance = GetDefaultGroundCheckDistance();
        }

        private void OnEnable() {
            InputHandler.Enable();
        }

        private void OnDisable() {
            InputHandler.Disable();
            OnStateDebugging(awake: false);
        }

        private void Update() {
            StateCtx.LocoStateHandler.CurrentLocoState.UpdateState();
            StateCtx.ActionStateHandler.CurrentActionState.UpdateState();
        }

        private void FixedUpdate() {
            StateCtx.LocoStateHandler.CurrentLocoState.FixedUpdateState();
            StateCtx.ActionStateHandler.CurrentActionState.FixedUpdateState();
            DoGroundCheck();
        }

        private void LateUpdate() {
            
        }

        /// <summary>
        /// Subscribing debugging methods to state change events.
        /// </summary>
        private void OnStateDebugging(bool awake) {
            if (!debugging) return;
            if (awake) {
                StateHelper.OnLocoStateChange += HandleLocoStateChanged;
                StateHelper.OnActionStateChange += HandleActionStateChanged;
            }
            else {
                StateHelper.OnLocoStateChange -= HandleLocoStateChanged;
                StateHelper.OnActionStateChange -= HandleActionStateChanged;
            }
        }
        
        /// <summary>
        /// Debugging States to SerializedField.
        /// </summary>
        private void HandleLocoStateChanged(BaseStateSO newState) {
            baseLocoStateSO = newState;
            Debug.Log($"Loco State SO changed to: {newState.GetType().Name}");
        }
        
        /// <summary>
        /// Debugging States to SerializedField.
        /// </summary>
        private void HandleActionStateChanged(BaseStateSO newState) {
            baseActionStateSO = newState;
            Debug.Log($"Action State SO changed to: {newState.GetType().Name}");
        }

        /// <summary>
        /// Ground Check Gizmo.
        /// </summary>
        private void OnDrawGizmosSelected() {
            if (!debugging) return;
            
            Debug.Log("Gizmo is running.");
            
            var origin = Rigidbody.transform.position + Vector3.up * 0.1f;
            var direction = Vector3.down * GroundCheckDistance;
            
            // Green if grounded, red if not.
            Gizmos.color = StateCtx.IsGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(origin, direction);
        }
        
        /// <summary>
        /// Called whenever movement input is received.
        /// </summary>
        public virtual void OnMove(Vector2 moveVector) {
            Debug.Log($"[BaseCharacterController] OnMove received: {moveVector}");
        }

        /// <summary>
        /// Checks to see if the player is grounded and then updates the context accordingly.
        /// </summary>
        protected virtual void DoGroundCheck() {
            StateCtx.SetGroundedStatus(RbHandler.GroundCheck(null, GroundCheckDistance, groundLayer));
        }
        
        /// <summary>
        /// Checks the sizing of the collider to determine a length for a ground check raycast.
        /// </summary>
        private float GetDefaultGroundCheckDistance() {
            const float buffer = 0.15f;

            if (Rigidbody.TryGetComponent(out CapsuleCollider capsule))
                return capsule.height / 2f + buffer;
            if (Rigidbody.TryGetComponent(out BoxCollider box))
                return box.bounds.extents.y + buffer;

            // Failsafe
            return 0.3f + buffer;
        }
    }
}