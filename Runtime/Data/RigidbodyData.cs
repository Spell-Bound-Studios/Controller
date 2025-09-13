using System;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    [Serializable]
    public class RigidbodyData {
        [Header("Rigidbody Settings:")]
        [field: SerializeField] public ForceMode horizontalForceMode = ForceMode.VelocityChange;
        [field: SerializeField] public ForceMode verticalForceMode = ForceMode.Impulse;
    }
}