using UnityEngine;
using UnityEngine.InputSystem;

namespace SpellBound.Controller.Configuration {
    public class PlayerController : MonoBehaviour {
        private InputActions _inputActions;
        private Vector2 _moveInput;
        
        private void Awake() {
            _inputActions = new InputActions();
        }

        private void OnEnable() {
            _inputActions.Enable();
            
            // Movement (WASD)
            _inputActions.PlayerInput.Movement.performed += OnMovePerformed;
            _inputActions.PlayerInput.Movement.canceled += OnMoveCanceled;
            
            // Jump (space bar)
            _inputActions.PlayerInput.Jump.performed += _ => OnJumpInput();
        }
        
        private void OnDisable() {
            _inputActions.Disable();
        }
        
        private void OnMovePerformed(InputAction.CallbackContext ctx) => _moveInput = ctx.ReadValue<Vector2>();
        private void OnMoveCanceled(InputAction.CallbackContext ctx) => _moveInput = Vector2.zero;
        
        private void OnJumpInput() {
            Debug.Log("Jumping.");
        }
    }
}