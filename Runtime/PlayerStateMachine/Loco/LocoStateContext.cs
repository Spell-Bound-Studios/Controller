using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public struct LocoStateContext {
        public Vector2 MoveInput;
        public float Speed;
        public bool Grounded;
    }
}