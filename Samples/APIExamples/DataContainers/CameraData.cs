// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class CameraData {
        [Header("Look Limits")]
        [field: SerializeField, Range(0f, 90f),
         Tooltip("How far up you can look (degrees from horizontal). Higher = see more sky; lower = restricts looking up.")]
        public float upperVerticalLimit { get; private set; } = 89f;

        [field: SerializeField, Range(0f, 90f),
         Tooltip("How far down you can look (degrees from horizontal). Higher = see more ground; lower = restricts looking down.")]
        public float lowerVerticalLimit { get; private set; } = 89f;

        [Header("Look Feel")]
        [field: SerializeField, Range(0f, 5f),
         Tooltip("Mouse/stick look sensitivity. Higher = camera turns faster per input; lower = slower, steadier aim.")]
        public float cameraSpeed { get; set; } = 0.5f;

        [field: SerializeField,
         Tooltip("Smooth (lerp) look input instead of applying it raw. On = softer, slightly trailing camera; off = 1:1 instant.")]
        public bool smoothCameraRotation { get; set; } = false;

        [field: SerializeField, Range(1f, 50f),
         Tooltip("Strength of look smoothing when Smooth Camera Rotation is on. Higher = snappier (less smoothing); lower = floatier.")]
        public float cameraSmoothingFactor { get; private set; } = 25f;

        [Header("Zoom")]
        [field: SerializeField, Range(0f, 2f),
         Tooltip("Distance the camera zooms per scroll notch. Higher = faster zoom steps; lower = finer zoom control.")]
        public float zoomIncrement { get; private set; } = .2f;

        [field: SerializeField, Range(0.1f, 20f),
         Tooltip("Closest the camera can zoom in. Lower = can get nearer the character; higher = stays farther out.")]
        public float minZoomDistance { get; private set; } = 1f;

        [field: SerializeField, Range(1f, 50f),
         Tooltip("Farthest the camera can zoom out. Higher = wider pulled-back view; lower = stays closer in.")]
        public float maxZoomDistance { get; private set; } = 8f;

        [Header("Startup")]
        [field: SerializeField,
         Tooltip("Lock and hide the cursor when play starts. On = mouse-look style; off = free cursor.")]
        public bool cursorLockOnStart { get; private set; } = true;

        [field: SerializeField,
         Tooltip("Whether the camera orbits with look input. On = look controls the camera; off = camera ignores look input.")]
        public bool cameraFollowMouse { get; set; } = true;
    }
}
