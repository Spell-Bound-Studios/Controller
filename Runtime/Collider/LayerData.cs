using System;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    [Serializable]
    public class LayerData {
        [field: SerializeField] public LayerMask GroundLayer { get; private set; } = 1 << 6;
    }
}