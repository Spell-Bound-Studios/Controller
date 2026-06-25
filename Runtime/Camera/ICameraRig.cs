// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// The central store for camera operations: which camera is live, switching/blending between them, and the
    /// target the cameras follow. <see cref="CameraRigManager"/> is the Cinemachine-backed implementation.
    /// </summary>
    public interface ICameraRig {
        /// <summary>The name of the camera currently shown (or being blended to); null if none.</summary>
        string Current { get; }

        /// <summary>The transform of the live camera — the reference for camera-relative movement.</summary>
        Transform CurrentCameraTransform { get; }

        /// <summary>Points every camera at <paramref name="target"/> — the consumer's runtime pivot.</summary>
        void SetFollowTarget(Transform target);

        /// <summary>Makes the camera named <paramref name="cameraName"/> the live one; the backend blends to it.</summary>
        void Switch(string cameraName);
    }
}
