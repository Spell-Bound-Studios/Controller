// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller {
    [Serializable]
    public class CapsuleFloatData {
        [Header("Ride Height")]
        [field: SerializeField, Range(0f, 5f)]
        public float DesiredFloatDistance { get; set; } = 1f;

        [field: SerializeField] public float CalculatedFloatDistance { get; private set; }
        [field: SerializeField] public bool OverrideCalculatedDistance { get; set; }

        [Header("Float Spring")]
        [field: SerializeField, Range(0f, 500f)]
        public float SpringStrength { get; set; } = 200f;

        [field: SerializeField, Range(0f, 100f)]
        public float SpringDamper { get; set; } = 25f;

        [Header("Ground Probe")]
        [field: SerializeField, Range(0.01f, 1f)]
        public float ProbeRadius { get; set; } = 0.25f;

        [field: SerializeField, Range(0f, 1f)]
        public float GroundedTolerance { get; set; } = 0.3f;

        [Header("Wall & Ceiling")]
        [field: SerializeField, Range(0f, 1f)]
        public float WallProbeDistance { get; set; } = 0.35f;

        [field: SerializeField, Range(0f, 1f)]
        public float CeilingClearance { get; set; } = 0.15f;

        public float RideHeight =>
                OverrideCalculatedDistance
                        ? DesiredFloatDistance
                        : CalculatedFloatDistance;

        public bool IsWithinGroundedRange(float distance) => distance <= RideHeight + GroundedTolerance;

        public void SetCalculatedFloatDistance(float distance) => CalculatedFloatDistance = distance;
    }
}
