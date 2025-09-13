using System;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    [Serializable]
    public class AutoResizeData {
        [Header("Auto Resize Settings")]
        [field: SerializeField] public bool EnableAutoResize { get; private set; } = true;
        [field: SerializeField] public float MeshHeightScale { get; private set; } = 1.0f;
        [field: SerializeField] public float MeshRadiusScale { get; private set; } = 1.0f;
    }
}