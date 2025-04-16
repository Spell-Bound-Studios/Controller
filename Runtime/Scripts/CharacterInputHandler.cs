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
            
            // Jump (space bar)
            _inputActions.PlayerInput.Jump.performed += _ => _controller.OnJumpInput();

            // Right Click

            // Left Click

            // Sprint (shift)

            // Hotkeys
        }

        public void Disable() {
            _inputActions.Disable();
        }
    }
}