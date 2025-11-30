using System;
using System.ComponentModel;
using UnityEngine;

namespace Spellbound.Controller.Samples
{
    [Serializable]
    public class CameraData
    {
        // How high you can move your camera.
        [field: SerializeField, Range(0f, 90f)] public float upperVerticalLimit { get; private set; } = 89f;

        // How low you can move your camera.
        [field: SerializeField, Range(0f, 90f)] public float lowerVerticalLimit { get; private set; } = 89f;

        // Multiplies the time.deltaTime by this factor.
        [field: SerializeField, Range(1f, 50f)] public float cameraSmoothingFactor { get; private set; } = 25f;

        // Controls how fast you zoom in and out.
        [field: SerializeField] public float zoomIncrement { get; private set; } = .2f;
        [field: SerializeField] public bool cursorLockOnStart { get; private set; } = true;
        [field: SerializeField] public bool cameraFollowMouse { get; set; } = true;

        [Header("Player Settings"), SerializeField, Description("How fast you can move your camera.")]
        public float cameraSpeed { get; set; } = 0.5f;

        [field: SerializeField, Description("Optional lerp applied to input.")]
        public bool smoothCameraRotation { get; set; } = false;

        [field: SerializeField, Min(0.1f)] public float minZoomDistance { get; private set; } = 1f;
        [field: SerializeField, Min(1f)] public float maxZoomDistance { get; private set; } = 8f;
    }
}