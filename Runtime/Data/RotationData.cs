using System;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    [Serializable]
    public class RotationData {
        [Header("Rotation Settings")]
        [field: SerializeField] public float turnTowardsInputSpeed { get; private set; } = 500f;
        [field: SerializeField] public float RotationFallOffAngle { get; private set; } = 90f;
    }
}