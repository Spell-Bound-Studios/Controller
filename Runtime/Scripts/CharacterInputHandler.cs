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
            
            _inputActions.PlayerInput.Movement.performed += c => _controller.OnMoveInput(c.ReadValue<Vector2>());
            _inputActions.PlayerInput.Movement.canceled += _ => _controller.OnMoveInput(Vector2.zero);
        }

        public void Disable() {
            _inputActions.Disable();
        }
    }
}