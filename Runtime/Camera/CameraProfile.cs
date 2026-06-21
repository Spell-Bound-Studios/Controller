// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Stable identity for one camera role (free-look, aiming, birds-eye, …). The rig maps a profile to a live
    /// Cinemachine camera; consumers switch cameras by profile. Inherits a guid + FNV-1a hash from
    /// <see cref="HashedScriptableObject"/> so a camera can be referenced, saved, and networked by one value and
    /// resolved through <see cref="CameraProfileRegistry"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraProfile", menuName = "Spellbound/Camera/CameraProfile")]
    public class CameraProfile : HashedScriptableObject {
        [field: SerializeField,
         Tooltip("Seconds to blend to this camera when switched to. Higher = slower, smoother transition; 0 = hard cut.")]
        public float BlendTime { get; set; } = 0.5f;

        [field: SerializeField,
         Tooltip("Min/max abstracted zoom (e.g. follow distance) for this camera. x = closest, y = farthest.")]
        public Vector2 ZoomRange { get; set; } = new(1f, 8f);
    }
}
