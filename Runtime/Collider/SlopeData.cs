// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    [Serializable]
    public class SlopeData {
        [field: SerializeField, Range(0f, 1f),
         Tooltip("Fraction of character height treated as step height; raises the collider center so small steps " +
                 "are climbable. Higher = steps over taller ledges; lower = trips on small steps.")]
        public float StepHeightPercentage { get; private set; } = 0.25f;

        [field: SerializeField, Range(0f, 5f),
         Tooltip("Length of the legacy straight-down ground ray (used by the game-logic step assist, not the " +
                 "floating-capsule probe). Higher = detects ground from farther up; lower = tighter detection.")]
        public float RayDistance { get; private set; } = 2f;

        [field: SerializeField, Range(0f, 50f),
         Tooltip("Strength of the legacy step-up assist that lifts the body onto steps. Higher = snappier step-ups " +
                 "(can pop); lower = gentler.")]
        public float StepReachForce { get; private set; } = 25f;

        private float _previousStepHeightPercentage;

        public void ClearStepHeightPercentage() {
            _previousStepHeightPercentage = StepHeightPercentage;
            StepHeightPercentage = 0f;
        }

        public void RevertStepHeightPercentage() => StepHeightPercentage = _previousStepHeightPercentage;
    }
}
