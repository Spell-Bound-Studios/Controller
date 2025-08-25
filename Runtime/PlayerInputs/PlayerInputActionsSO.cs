using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions;

namespace SpellBound.Controller.PlayerInputs {
    [CreateAssetMenu(fileName = "PlayerInputs", menuName = "Spellbound/PlayerInputs/PlayerInputs")]
    public class PlayerInputActionsSO : ScriptableObject, IPlayerInputActions {
        private InputActions _inputActions;
        
        public event Action<Vector2> OnMoveInput = delegate { };
        public event Action<Vector2> OnLookInput = delegate { };
        public event Action<Vector2> OnMouseWheelInput = delegate { };
        public event Action OnJumpInput = delegate { };
        
        
        public Vector3 Direction => _inputActions.PlayerInput.Movement.ReadValue<Vector2>();
        public Vector3 LookDirection => _inputActions.PlayerInput.LookDirection.ReadValue<Vector2>();
        public float MouseWheelValue => _inputActions.PlayerInput.MouseWheel.ReadValue<float>();

        private void OnEnable() {
            if (_inputActions == null) {
                _inputActions = new InputActions();
                _inputActions.PlayerInput.SetCallbacks(this);
            }
            
            _inputActions.Enable();
        }

        private void OnDisable() {
            _inputActions.Disable();
        }
        
        public void OnMovement(InputAction.CallbackContext context) {
            OnMoveInput.Invoke(context.ReadValue<Vector2>());
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.performed)
                OnJumpInput.Invoke();
        }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnLeftClick(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }
        public void OnHotkeyOne(InputAction.CallbackContext context) { }
        public void OnHotkeyTwo(InputAction.CallbackContext context) { }
        public void OnHotkeyThree(InputAction.CallbackContext context) { }
        public void OnHotkeyFour(InputAction.CallbackContext context) { }
        public void OnCharacterMenu(InputAction.CallbackContext context) { }
        public void OnSettingsMenu(InputAction.CallbackContext context) { }
        public void OnInventory(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) { }

        public void OnLookDirection(InputAction.CallbackContext context) {
            OnLookInput.Invoke(context.ReadValue<Vector2>());
        }

        public void OnMouseWheel(InputAction.CallbackContext context) {
            OnMouseWheelInput.Invoke(context.ReadValue<Vector2>());
        }
    }
}