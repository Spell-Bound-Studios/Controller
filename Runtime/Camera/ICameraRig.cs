// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// The central store for camera operations: which camera is live, switching/blending between them, and the
    /// abstracted zoom. <see cref="CameraRigManager"/> is the Cinemachine-backed implementation.
    /// </summary>
    public interface ICameraRig {
        /// <summary>The profile of the camera currently shown (or being blended to).</summary>
        CameraProfile Current { get; }

        /// <summary>The transform of the live camera — the reference for camera-relative movement.</summary>
        Transform CurrentCameraTransform { get; }

        /// <summary>Abstracted zoom (e.g. follow distance); <c>NaN</c> if the live camera has no zoom concept.</summary>
        float Zoom { get; set; }

        /// <summary>Makes the camera mapped to <paramref name="profile"/> the live one; the backend blends to it.</summary>
        void Switch(CameraProfile profile);
    }
}
