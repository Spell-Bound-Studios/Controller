// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class StatData {
        [Header("Stat Values")]
        [field: SerializeField]
        public float movementSpeed { get; private set; } = 5f;

        [field: SerializeField] public float jumpForce { get; private set; } = 5f;
        [field: SerializeField] public float JumpMultiplier { get; private set; } = 1f;
        [field: SerializeField] public float gravity { get; private set; } = -20f;
        [field: SerializeField] public float slopeSpeedModifier { get; set; } = 1f;

        // Unused atm
        [field: SerializeField] public float TerminalSlidingSpeed { get; private set; } = 50f;
        [field: SerializeField, Range(0f, 5f)] public float slideAccelMultiplier { get; private set; } = 2.0f;

        [field: SerializeField, Range(0f, 20f)]
        public float lateralSteerAccel { get; private set; } = 8.0f;

        [field: SerializeField, Range(0f, 10f)]
        public float planarDrag { get; private set; } = 1.2f;
    }
}