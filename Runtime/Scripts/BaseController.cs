using PurrNet;
using SpellBound.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// Base Controller class to be inherited by a component class. This will do a lot of behind the scenes
    /// for the user. Alternatively, they will be given full control for every function if they want to change it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class BaseController : NetworkBehaviour {
        // Handlers
        protected CharacterInputHandler InputHandler;
        
        // State Machine
        protected StateContext StateCtx;
        
        // Animation Controller
        protected AnimationController AnimationController;
        
        protected void Awake() {
            // Inputs
            InputHandler = new CharacterInputHandler(this);
            
            // Create State Context
            CreateStateContext();
            
            // Create the animation controller
            CreateAnimationController();
        }

        protected void OnEnable() {
            InputHandler.Enable();
        }

        protected void OnDisable() {
            InputHandler.Disable();
            AnimationController.DisposeEvents();
        }
        
        /// <summary>
        /// Called whenever movement input is received.
        /// </summary>
        public virtual void OnMoveInput(Vector2 moveVector) {
            StateCtx.InputVector = moveVector;
        }
        
        /// <summary>
        /// Called whenever jump key is pressed.
        /// </summary>
        public virtual void OnJumpInput() {
            StateCtx.OnJumpInput?.Invoke();
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
            StateCtx = gameObject.AddComponent<StateContext>();
            StateCtx.cameraTransform = FindCameraTransform();
            Debug.Log("Setting StateCtx.cameraTransform");
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