// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace SpellBound.Controller {
    public sealed class InputManager : MonoBehaviour {
        public static InputManager Instance;
        [SerializeField] private PlayerInputActionsSO playerInputActionsSO;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);
        }

        public PlayerInputActionsSO GetInputs() => playerInputActionsSO;
    }
}