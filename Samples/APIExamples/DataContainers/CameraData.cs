// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    [Serializable]
    public class CameraData {
        [Header("Look Limits")]
        [field: SerializeField, Range(0f, 90f)]
        public float upperVerticalLimit { get; private set; } = 89f;

        [field: SerializeField, Range(0f, 90f)]
        public float lowerVerticalLimit { get; private set; } = 89f;

        [Header("Look Feel")]
        [field: SerializeField, Range(0f, 5f), Tooltip("How fast you can move your camera.")]
        public float cameraSpeed { get; set; } = 0.5f;

        [field: SerializeField, Tooltip("Optional lerp applied to look input.")]
        public bool smoothCameraRotation { get; set; } = false;

        [field: SerializeField, Range(1f, 50f)]
        public float cameraSmoothingFactor { get; private set; } = 25f;

        [Header("Zoom")]
        [field: SerializeField, Range(0f, 2f)]
        public float zoomIncrement { get; private set; } = .2f;

        [field: SerializeField, Range(0.1f, 20f)] public float minZoomDistance { get; private set; } = 1f;
        [field: SerializeField, Range(1f, 50f)] public float maxZoomDistance { get; private set; } = 8f;

        [Header("Startup")]
        [field: SerializeField] public bool cursorLockOnStart { get; private set; } = true;

        [field: SerializeField] public bool cameraFollowMouse { get; set; } = true;
    }
}
