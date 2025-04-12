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
        protected LocoStateHandler LocoStateHandler;
        protected ActionStateHandler ActionStateHandler;
        
        // Debugging Only
        [SerializeField] private bool debugging;
        [SerializeField] private BaseStateSO baseActionStateSO;
        [SerializeField] private BaseStateSO baseLocoStateSO;
        
        private void Awake() {
            // Debugging
            OnDebugging(enable: true);
            
            // Rigidbody
            Rigidbody = GetComponent<Rigidbody>();
            RbHandler = new RigidbodyHandler();
            RbHandler.InitializeRigidbody(Rigidbody);
            
            // Inputs
            InputHandler = new CharacterInputHandler(this);
            
            // State Machine: Create state handlers so that you can inject them into the BaseStates.
            LocoStateHandler = new LocoStateHandler(this);
            ActionStateHandler = new ActionStateHandler(this);
            
            // Now that each SO and base state exists we need to kick off the handlers with some default state entries.
            LocoStateHandler.Initialize(null);
            ActionStateHandler.Initialize(null);
        }

        private void OnEnable() {
            InputHandler.Enable();
        }

        private void OnDisable() {
            InputHandler.Disable();
            OnDebugging(enable: false);
        }

        private void Update() {
            LocoStateHandler.CurrentLocoState.UpdateState();
            ActionStateHandler.CurrentActionState.UpdateState();
        }

        private void FixedUpdate() {
            LocoStateHandler.CurrentLocoState.FixedUpdateState();
            ActionStateHandler.CurrentActionState.FixedUpdateState();
        }

        private void LateUpdate() {
            
        }

        /// <summary>
        /// Debugging
        /// </summary>
        private void OnDebugging(bool enable) {
            if (!debugging) return;
            if (enable) {
                StateHelper.OnLocoStateChange += HandleLocoStateChanged;
                StateHelper.OnActionStateChange += HandleActionStateChanged;
            }
            else {
                StateHelper.OnLocoStateChange -= HandleLocoStateChanged;
                StateHelper.OnActionStateChange -= HandleActionStateChanged;
            }
        }
        
        private void HandleLocoStateChanged(BaseStateSO newState) {
            baseLocoStateSO = newState;
            Debug.Log($"Loco State SO changed to: {newState.GetType().Name}");
        }
        
        private void HandleActionStateChanged(BaseStateSO newState) {
            baseActionStateSO = newState;
            Debug.Log($"Action State SO changed to: {newState.GetType().Name}");
        }
        
        /// <summary>
        /// Called whenever movement input is received.
        /// </summary>
        public virtual void OnMove(Vector2 moveVector) {
            Debug.Log($"[BaseCharacterController] OnMove received: {moveVector}");
        }
    }
}