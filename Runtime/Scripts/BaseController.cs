using SpellBound.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// Base Controller class to be inherited by a component class. This will do a lot of behind the scenes
    /// for the user. Alternatively, they will be given full control for every function if they want to change it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseController : MonoBehaviour {
        // Components
        protected Rigidbody Rigidbody;
        
        // Handlers
        protected CharacterInputHandler InputHandler;
        
        // State Machine
        protected StateContext StateCtx;
        
        // Animation Controller
        protected AnimationController AnimationController;
        
        // Ground Checking
        [SerializeField] protected LayerMask groundLayer;
        protected float GroundCheckDistance;
        
        // Debugging Only
        [SerializeField] private bool debugging;
        [SerializeField] private BaseStateSO baseActionStateSO;
        [SerializeField] private BaseStateSO baseLocoStateSO;
        
        protected void Awake() {
            // Debugging
            OnStateDebugging(awake: true);
            
            // Rigidbody
            Rigidbody = GetComponent<Rigidbody>();
            
            // Inputs
            InputHandler = new CharacterInputHandler(this);
            
            // Create state context to inject into the state handlers.
            CreateStateContext();
            
            // Create the animation controller
            CreateAnimationController();
            
            // Ground Checks
            GroundCheckDistance = GetDefaultGroundCheckDistance();
        }

        protected void OnEnable() {
            InputHandler.Enable();
        }

        protected void OnDisable() {
            InputHandler.Disable();
            AnimationController.DisposeEvents();
            OnStateDebugging(awake: false);
        }
        
        private void Update() {
            StateCtx.LocoStateHandler.CurrentLocoState.UpdateState();
            StateCtx.ActionStateHandler.CurrentActionState.UpdateState();
        }
        
        protected void FixedUpdate() {
            StateCtx.LocoStateHandler.CurrentLocoState.FixedUpdateState();
            StateCtx.ActionStateHandler.CurrentActionState.FixedUpdateState();
            DoGroundCheck();
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
            
            var origin = Rigidbody.transform.position + Vector3.up * 0.1f;
            var direction = Vector3.down * GroundCheckDistance;
            
            // Green if grounded, red if not.
            Gizmos.color = StateCtx.IsGrounded ? Color.green : Color.red;
            Gizmos.DrawRay(origin, direction);
        }
        
        /// <summary>
        /// Called whenever movement input is received.
        /// </summary>
        public virtual void OnMoveInput(Vector2 moveVector) {
            StateCtx.InputVector = moveVector;
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
        
        /// <summary>
        /// Checks to see if the player is grounded and then updates the context accordingly.
        /// </summary>
        protected virtual void DoGroundCheck() {
            StateCtx.IsGrounded = GroundCheck(null, GroundCheckDistance, groundLayer);
        }
        
        /// <summary>
        /// Performs a ground check using a downward raycast.
        /// </summary>
        protected bool GroundCheck(
            Transform groundCheckTransform,
            float checkDistance = 1.5f,
            LayerMask groundLayerMask = default,
            float originYOffset = 0.1f
        ) {
            var trans = groundCheckTransform ?? Rigidbody.transform;
            
            if (trans == null) {
                Debug.LogWarning("[RigidbodyHandler]: Requires a rigidbody or transform. Both were null.");
                return false;
            }

            var origin = trans.position + Vector3.up * originYOffset;
            var hit = Physics.Raycast(origin, Vector3.down, checkDistance, groundLayerMask);

            return hit;
        }
        
        /// <summary>
        /// Creates a CameraComponent and adds it to this gameobject and gets its transform for tracking.
        /// </summary>
        protected virtual Transform FindCameraTransform() {
            var cameraComponent = gameObject.AddComponent<CameraComponent>();
            return cameraComponent.Camera.transform;
        }
        
        /// <summary>
        /// Responsible for creating the StateContext which in turn kicks off the State Machine logic.
        /// </summary>
        protected virtual void CreateStateContext() {
            StateCtx = new StateContext(Rigidbody, FindCameraTransform());
        }

        /// <summary>
        /// Responsible for creating the animation controller which hooks up directly to the animator and subscribes
        /// to StateContext events.
        /// </summary>
        protected virtual void CreateAnimationController() {
            var animator = GetComponentInChildren<Animator>();
            AnimationController = new AnimationController(animator, StateCtx);
            StateCtx.OnStateChanged?.Invoke(StateContext.DefaultState);
        }
    }
}