// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [Serializable]
    public class StateData {
        [field: SerializeField] public bool Grounded { get; set; }
        [field: SerializeField] public AnimationCurve SlopeSpeedCurve { get; private set; }
    }
}