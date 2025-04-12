using UnityEngine;

namespace SpellBound.CharacterController {
    /// <summary>
    /// Handles all player input.
    /// </summary>
    public class CharacterInputHandler {
        private readonly BaseCharacterController _controller;
        private readonly InputActions _inputActions = new();
        
        // Constructor
        public CharacterInputHandler(BaseCharacterController controller) {
            _controller = controller;
        }
        
        public void Enable() {
            _inputActions.Enable();
            
            _inputActions.PlayerInput.Movement.performed += c => _controller.OnMove(c.ReadValue<Vector2>());
            _inputActions.PlayerInput.Movement.canceled += _ => _controller.OnMove(Vector2.zero);
        }

        public void Disable() {
            _inputActions.Disable();
        }
    }
}