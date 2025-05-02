using System;
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
        
        // Camera Component
        public CameraComponent cameraComponent;

        /// <summary>
        /// InputHandler invokes OnMenuPressed because it's a POCO - so it reaches into this script.
        /// Currently subscribed to in CharacterPanel.cs
        /// </summary>
        public Action OnMenuPressed;

        public Action OnSettingsPressed;

        public Action OnMouseButtonClicked;

        protected override void OnSpawned() {
            base.OnSpawned();
            
            Debug.Log("#################################################");
            Debug.Log($"[BaseController] Hello, I am the host: {isHost}");
            Debug.Log($"[BaseController] Hello, I am the server: {isServer}");
            Debug.Log($"[BaseController] Hello, I am the client: {isClient}");
            Debug.Log($"[BaseController] Hello, I am the owner: {isOwner}");
            Debug.Log("##################################################");
            
            // If we are not the client then exit. This ensures that each client runs a base implementation of BaseController.
            if (!isOwner) {
                return;
            }

            // Redundancy to ensure this script is enabled.
            enabled = isOwner;
            
            // Creates and subscribes the input actions mapping to methods in this script.
            InputHandler = new CharacterInputHandler(this);
            
            // Creates a Monobehaviour component responsible for managing character state. Also creates a camera component.
            CreateStateContext();
            
            // Set the camera of the StateCtx so that states can use it.
            var camTran = FindCameraTransform();
            if (camTran != null) {
                StateCtx.cameraTransform = camTran;
            } else Debug.LogWarning("No camera found! ITS NULL");
            
            // Creates a POCO that handles which blend tree to .Play, SetFloat, SetBool, etc.
            CreateAnimationController();
            
            InputHandler.Enable();
        }

        protected override void OnDespawned() {
            base.OnDespawned();
            if (!isOwner) return;
            
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
        /// Called on Right Click.
        /// </summary>
        public virtual void OnRightMouseClick(bool clicked) {
            
        }

        /// <summary>
        /// Called on Left Click.
        /// </summary>
        public virtual void OnLeftMouseClick(bool clicked) {
            OnMouseButtonClicked?.Invoke();
        }

        /// <summary>
        /// Called on shift held.
        /// </summary>
        public virtual void OnSprintHeld(bool held) {
            
        }

        /// <summary>
        /// Called when the left most hotkey bind is pressed.
        /// </summary>
        public virtual void OnHotkeyOnePressed(string hotkey) {
            StateCtx.OnHotkeyAction?.Invoke(hotkey);
        }
        
        /// <summary>
        /// Called when the left most hotkey bind + one is pressed.
        /// </summary>
        public virtual void OnHotkeyTwoPressed(string hotkey) {
            StateCtx.OnHotkeyAction?.Invoke(hotkey);
        }
        
        /// <summary>
        /// Called when the left most hotkey bind + two is pressed.
        /// </summary>
        public virtual void OnHotkeyThreePressed(string hotkey) {
            StateCtx.OnHotkeyAction?.Invoke(hotkey);
        }
        
        /// <summary>
        /// Called when the left most hotkey bind + three is pressed.
        /// </summary>
        public virtual void OnHotkeyFourPressed(string hotkey) {
            StateCtx.OnHotkeyAction?.Invoke(hotkey);
        }
        
        /// <summary>
        /// Creates a CameraComponent and adds it to this gameobject and gets its transform for tracking.
        /// </summary>
        protected virtual Transform FindCameraTransform() {
            cameraComponent = gameObject.AddComponent<CameraComponent>();
            return cameraComponent.Camera.transform;
        }
        
        /// <summary>
        /// Responsible for creating the StateContext which in turn kicks off the State Machine logic.
        /// </summary>
        protected virtual void CreateStateContext() {
            StateCtx = gameObject.AddComponent<StateContext>();
        }

        /// <summary>
        /// Responsible for creating the animation controller which hooks up directly to the animator and subscribes
        /// to StateContext events.
        /// </summary>
        protected virtual void CreateAnimationController() {
            var networkAnimator = GetComponentInChildren<NetworkAnimator>();
            AnimationController = new AnimationController(networkAnimator, StateCtx);
            StateCtx.OnStateChanged?.Invoke(StateContext.DefaultState);
        }
    }
}