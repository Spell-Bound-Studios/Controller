// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static InputActions;

namespace Spellbound.Controller {
    [CreateAssetMenu(fileName = "PlayerInputs", menuName = "Spellbound/PlayerInputs/PlayerInputs")]
    public class PlayerInputActionsSO : ScriptableObject, IPlayerInputActions {
        private InputActions _inputActions;

        public event Action<Vector2> OnMouseWheelInput = delegate { };
        public event Action OnJumpInput = delegate { };
        public event Action OnInteractPressed = delegate { };
        public event Action OnInventoryPressed = delegate { };
        public event Action OnHotkeyOnePressed = delegate { };
        public event Action OnHotkeyTwoPressed = delegate { };
        public event Action OnHotkeyThreePressed = delegate { };
        public event Action OnHotkeyFourPressed = delegate { };
        public event Action OnEscPressed = delegate { };
        public event Action OnCharacterMenuPressed = delegate { };
        public event Action OnFOnePressed = delegate { };
        public event Action OnFTwoPressed = delegate { };

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

        private void OnDisable() => _inputActions.Disable();

        public void OnMovement(InputAction.CallbackContext context) { }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.performed)
                OnJumpInput.Invoke();
        }

        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnLeftClick(InputAction.CallbackContext context) { }
        public void OnSprint(InputAction.CallbackContext context) { }

        public void OnHotkeyOne(InputAction.CallbackContext context) {
            if (context.performed)
                OnHotkeyOnePressed.Invoke();
        }

        public void OnHotkeyTwo(InputAction.CallbackContext context) {
            if (context.performed)
                OnHotkeyTwoPressed.Invoke();
        }

        public void OnHotkeyThree(InputAction.CallbackContext context) {
            if (context.performed)
                OnHotkeyThreePressed.Invoke();
        }

        public void OnHotkeyFour(InputAction.CallbackContext context) {
            if (context.performed)
                OnHotkeyFourPressed.Invoke();
        }

        public void OnCharacterMenu(InputAction.CallbackContext context) {
            if (context.performed)
                OnCharacterMenuPressed.Invoke();
        }

        public void OnSettingsMenu(InputAction.CallbackContext context) {
            if (context.performed)
                OnEscPressed.Invoke();
        }

        public void OnInventory(InputAction.CallbackContext context) {
            if (context.performed)
                OnInventoryPressed.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context) {
            if (context.performed)
                OnInteractPressed.Invoke();
        }

        public void OnLookDirection(InputAction.CallbackContext context) { }

        public void OnMouseWheel(InputAction.CallbackContext context) =>
                OnMouseWheelInput.Invoke(context.ReadValue<Vector2>());

        public void OnFOne(InputAction.CallbackContext context) {
            if (context.performed)
                OnFOnePressed.Invoke();
        }
        
        public void OnFTwo(InputAction.CallbackContext context) {
            if (context.performed)
                OnFTwoPressed.Invoke();
        }
    }
}