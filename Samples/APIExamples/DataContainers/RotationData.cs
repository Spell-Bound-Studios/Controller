// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class RotationData {
        [Header("Rotation Settings")]
        [field: SerializeField, Range(0f, 1000f)]
        public float turnTowardsInputSpeed { get; set; } = 500f;

        [field: SerializeField, Range(0f, 180f)]
        public float RotationFallOffAngle { get; set; } = 90f;
    }
}
