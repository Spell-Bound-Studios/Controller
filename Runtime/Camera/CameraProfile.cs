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
    public class CameraProfile : HashedScriptableObject { }
}
