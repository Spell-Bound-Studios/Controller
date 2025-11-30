using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Spellbound.Controller.Samples
{
    /// <summary>
    /// The event naming pattern should be: [Context][Action][Suffix]
    /// </summary>
    public class ExampleInputManager : MonoBehaviour, ExampleInputActions.IPlayerInputActions, ExampleInputActions.IConsoleInputActions {
        private ExampleInputActions _inputActions;

        // Player Input Events
        public event Action OnJumpPressed = delegate { };
        public event Action OnInteractPressed = delegate { };
        public event Action<Vector2> OnMouseWheelInput = delegate { };
        
        // Console Input Events
        public event Action OnPreviousCommandPressed = delegate { };
        public event Action OnNextCommandPressed = delegate { };
        public event Action OnForceClosePressed = delegate { };
        public event Action OnToggleClosePressed = delegate { };

        #region Unity Lifecycle

        private void Awake() {
            _inputActions = new ExampleInputActions();
            _inputActions.PlayerInput.SetCallbacks(this);
            _inputActions.ConsoleInput.SetCallbacks(this);
        }
        
        private void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new ExampleInputActions();
                _inputActions.PlayerInput.SetCallbacks(this);
            }

            _inputActions.Enable();
        }

        private void OnDisable() => _inputActions?.Disable();
        
        private void OnDestroy() {
            _inputActions?.Dispose();
        }
        
        #endregion

        #region Swapping Input Maps

        public void SwitchToPlayerInput() {
            _inputActions.ConsoleInput.Disable();
            _inputActions.PlayerInput.Enable();
        }

        public void SwitchToConsoleInput() {
            _inputActions.PlayerInput.Disable();
            _inputActions.ConsoleInput.Enable();
        }

        #endregion
        
        #region PlayerInputs
        
        public Vector3 Direction => _inputActions.PlayerInput.Movement.ReadValue<Vector2>();
        public Vector3 LookDirection => _inputActions.PlayerInput.LookDirection.ReadValue<Vector2>();
        public float MouseWheelValue => _inputActions.PlayerInput.MouseWheel.ReadValue<float>();

        public void OnMovement(InputAction.CallbackContext context) { }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnJumpPressed.Invoke();
            }
        }

        public void OnLookDirection(InputAction.CallbackContext context) { }

        public void OnMouseWheel(InputAction.CallbackContext context) =>
            OnMouseWheelInput.Invoke(context.ReadValue<Vector2>());

        #endregion
        
        #region ConsoleInputs

        public void OnClose(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnToggleClosePressed.Invoke();
        }

        public void OnForceClose(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnForceClosePressed.Invoke();
        }

        public void OnPageUp(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnPreviousCommandPressed.Invoke();
        }

        public void OnPageDown(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnNextCommandPressed.Invoke();
        }
        
        #endregion
    }
}