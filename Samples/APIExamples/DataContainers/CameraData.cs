// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// The sample's serialized camera settings — implements <see cref="ICameraSettings"/> so any UI can drive the
    /// look/feel live, plus the cursor and follow toggles the sample Ctx reads.
    /// </summary>
    [Serializable]
    public class CameraData : ICameraSettings {
        [Header("Look Sensitivity")]
        [field: SerializeField, Range(0f, 5f),
         Tooltip("Horizontal look sensitivity. Higher = faster left/right turning; lower = slower.")]
        public float SensitivityX { get; set; } = 0.5f;

        [field: SerializeField, Range(0f, 5f),
         Tooltip("Vertical look sensitivity. Higher = faster up/down pitch; lower = slower.")]
        public float SensitivityY { get; set; } = 0.5f;

        [field: SerializeField,
         Tooltip("Invert vertical look (push up to look down). On = inverted; off = standard.")]
        public bool InvertY { get; set; }

        [Header("Smoothing")]
        [field: SerializeField,
         Tooltip("Smooth (lerp) look toward the input instead of applying it raw. On = softer; off = 1:1 instant.")]
        public bool SmoothLook { get; set; }

        [field: SerializeField, Range(1f, 50f),
         Tooltip("Look smoothing strength when Smooth Look is on. Higher = snappier (less smoothing); lower = floatier.")]
        public float SmoothingFactor { get; set; } = 25f;

        [Header("Pitch Limits")]
        [field: SerializeField, Range(-89f, 0f),
         Tooltip("Lowest pitch in degrees (how far down you can look). More negative = look further down.")]
        public float MinPitch { get; set; } = -89f;

        [field: SerializeField, Range(0f, 89f),
         Tooltip("Highest pitch in degrees (how far up you can look). Higher = look further up.")]
        public float MaxPitch { get; set; } = 89f;

        [Header("Cursor")]
        [field: SerializeField,
         Tooltip("Lock and hide the cursor on start for mouse-look. On = FPS-style; off = free cursor.")]
        public bool LockCursorOnStart { get; set; } = true;

        [field: SerializeField,
         Tooltip("Whether look input turns the camera. On = mouse-look active; off = camera ignores look.")]
        public bool FollowMouse { get; set; } = true;
    }
}
