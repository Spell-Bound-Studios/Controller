using System;
using UnityEngine;

namespace Spellbound.Controller.PlayerController {
    [Serializable]
    public class StateData {
        [field: SerializeField] public bool Grounded { get; set; }
        [field: SerializeField] public AnimationCurve SlopeSpeedCurve { get; private set; }
    }
}