using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// Handles all player input.
    /// </summary>
    public class CharacterInputHandler {
        private readonly BaseController _controller;
        private readonly InputActions _inputActions = new();
        
        // Constructor
        public CharacterInputHandler(BaseController controller) {
            _controller = controller;
        }
        
        public void Enable() {
            _inputActions.Enable();
            
            // Movement (WASD)
            _inputActions.PlayerInput.Movement.performed += c => _controller.OnMoveInput(c.ReadValue<Vector2>());
            _inputActions.PlayerInput.Movement.canceled += _ => _controller.OnMoveInput(Vector2.zero);
            
            // Interact (E)
            _inputActions.PlayerInput.Interact.performed += _ => _controller.OnInteractPressed();
            
            // Jump (space bar)
            _inputActions.PlayerInput.Jump.performed += _ => _controller.OnJumpInput();

            // Right Click
            _inputActions.PlayerInput.RightClick.performed += _ => _controller.OnRightMouseClick(true);
            _inputActions.PlayerInput.RightClick.canceled += _ => _controller.OnRightMouseClick(false);
            
            // Left Click
            _inputActions.PlayerInput.LeftClick.performed += _ => _controller.OnLeftMouseClick(true);
            _inputActions.PlayerInput.LeftClick.canceled += _ => _controller.OnLeftMouseClick(false);

            // Sprint (shift)
            _inputActions.PlayerInput.Sprint.performed += _ => _controller.OnSprintPressed?.Invoke(true);
            _inputActions.PlayerInput.Sprint.canceled += _ => _controller.OnSprintPressed?.Invoke(false);
            
            // Hotkeys
            _inputActions.PlayerInput.HotkeyOne.performed += c => _controller.OnHotkeyOnePressed(c.control.displayName);
            _inputActions.PlayerInput.HotkeyTwo.performed += c => _controller.OnHotkeyTwoPressed(c.control.displayName);
            _inputActions.PlayerInput.HotkeyThree.performed += c => _controller.OnHotkeyThreePressed(c.control.displayName);
            _inputActions.PlayerInput.HotkeyFour.performed += c => _controller.OnHotkeyFourPressed(c.control.displayName);
            
            // UI
            _inputActions.PlayerInput.CharacterMenu.performed += _ => _controller.OnMenuPressed?.Invoke();
            _inputActions.PlayerInput.SettingsMenu.performed += _ => _controller.OnSettingsPressed?.Invoke();
            _inputActions.PlayerInput.Inventory.performed += _ => _controller.OnInventoryPressed?.Invoke();
        }

        public void Disable() {
            _inputActions.Disable();
        }
    }
}