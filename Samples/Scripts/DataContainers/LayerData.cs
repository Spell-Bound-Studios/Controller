using System;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [Serializable]
    public class LayerData {
        [field: SerializeField] public LayerMask GroundLayer { get; private set; } = 1 << 6;
    }
}