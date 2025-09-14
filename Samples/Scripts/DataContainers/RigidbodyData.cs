using System;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [Serializable]
    public class RigidbodyData {
        [Header("Rigidbody Settings:")]
        [field: SerializeField] public ForceMode horizontalForceMode = ForceMode.VelocityChange;
        [field: SerializeField] public ForceMode verticalForceMode = ForceMode.Impulse;
    }
}