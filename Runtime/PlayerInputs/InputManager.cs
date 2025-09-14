using UnityEngine;

namespace SpellBound.Controller {
    public class InputManager : MonoBehaviour {
        public static InputManager Instance;
        [SerializeField] private PlayerInputActionsSO playerInputActionsSO;
        
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }

        public PlayerInputActionsSO GetInputs() => playerInputActionsSO;
    }
}