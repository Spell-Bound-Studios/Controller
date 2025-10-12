// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace SpellBound.Controller {
    [Serializable]
    public class SlopeData {
        [field: SerializeField, Range(0f, 1f)] public float StepHeightPercentage { get; private set; } = 0.25f;
        [field: SerializeField, Range(0f, 5f)] public float RayDistance { get; private set; } = 2f;

        [field: SerializeField, Range(0f, 50f)]
        public float StepReachForce { get; private set; } = 25f;

        private float _previousStepHeightPercentage;

        public void ClearStepHeightPercentage() {
            _previousStepHeightPercentage = StepHeightPercentage;
            StepHeightPercentage = 0f;
        }

        public void RevertStepHeightPercentage() => StepHeightPercentage = _previousStepHeightPercentage;
    }
}