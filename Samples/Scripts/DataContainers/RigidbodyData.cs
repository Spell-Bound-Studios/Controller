// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class RigidbodyData {
        [Header("Rigidbody Settings:")] [field: SerializeField]
        public ForceMode horizontalForceMode = ForceMode.VelocityChange;

        [field: SerializeField] public ForceMode verticalForceMode = ForceMode.Impulse;
    }
}