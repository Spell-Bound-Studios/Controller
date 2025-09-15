using System;
using UnityEngine;

namespace SpellBound.Controller {
    [Serializable]
    public class CapsuleFloatData {
        [Header("Collider Configuration")]
        [field: SerializeField] public float DesiredFloatDistance { get; private set; }
        [field: SerializeField] public float CalculatedFloatDistance { get; private set; }
        [field: SerializeField] public bool OverrideCalculatedDistance { get; private set; }
    }
}