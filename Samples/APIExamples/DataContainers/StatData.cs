// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class StatData {
        [Header("Stat Values")]
        [field: SerializeField, Range(0f, 20f)]
        public float movementSpeed { get; set; } = 5f;

        [field: SerializeField, Range(0f, 30f)] public float jumpForce { get; set; } = 5f;
        [field: SerializeField, Range(0f, 5f)] public float JumpMultiplier { get; set; } = 1f;
        [field: SerializeField, Range(-50f, 0f)] public float gravity { get; set; } = -20f;

        [Header("Slope")]
        [field: SerializeField, Range(0f, 89f)]
        public float maxSlopeAngle { get; set; } = 50f;

        [field: SerializeField, Tooltip("x: -1 full uphill, 0 flat/traverse, +1 full downhill. y: speed multiplier.")]
        public AnimationCurve slopeSpeedCurve { get; set; } =
                new(new Keyframe(-1f, 0.25f), new Keyframe(0f, 1f), new Keyframe(1f, 1.3f));

        [Header("Slide")]
        [field: SerializeField, Range(0f, 100f)]
        public float TerminalSlidingSpeed { get; set; } = 50f;

        [field: SerializeField, Range(0f, 5f)] public float slideAccelMultiplier { get; set; } = 2.0f;

        [field: SerializeField, Range(0f, 20f)]
        public float lateralSteerAccel { get; set; } = 8.0f;

        [field: SerializeField, Range(0f, 10f)]
        public float planarDrag { get; set; } = 1.2f;
    }
}
