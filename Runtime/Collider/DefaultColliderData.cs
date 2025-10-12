// Copyright 2025 Spellbound Studio Inc.

using System;
using UnityEngine;

namespace SpellBound.Controller {
    [Serializable]
    public class DefaultColliderData {
        [Header("Collider Configuration")]
        [field: SerializeField]
        public float Height { get; private set; }

        public float CenterY { get; private set; }
        [field: SerializeField] public float Radius { get; private set; }

        // If the user doesn't want to automatically calculate based on mesh bounds then set to true.
        [Header("Override Settings")]
        [field: SerializeField]
        public bool OverrideHeight { get; private set; }

        [field: SerializeField] public bool OverrideRadius { get; private set; }

        private const float FallbackHeight = 1.8f;
        private const float FallbackRadius = 0.3f;

        [Header("Log Suppression")]
        [field: SerializeField]
        public bool SuppressWarnings { get; private set; }

        [field: SerializeField] public bool SuppressConsole { get; private set; } = true;

        public void Initialize(GameObject go) {
            Bounds bounds = default;
            var foundMesh = false;
            var meshSource = "";

            var smr = go.GetComponent<SkinnedMeshRenderer>();

            if (smr != null && smr.sharedMesh != null) {
                bounds = smr.sharedMesh.bounds;
                foundMesh = true;
                meshSource = "SkinnedMeshRenderer";
            }
            else {
                smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

                if (smr != null && smr.sharedMesh != null) {
                    bounds = smr.sharedMesh.bounds;
                    foundMesh = true;
                    meshSource = "SkinnedMeshRenderer (child)";
                }
                else {
                    var meshFilter = go.GetComponent<MeshFilter>();

                    if (meshFilter != null && meshFilter.sharedMesh != null) {
                        bounds = meshFilter.sharedMesh.bounds;
                        foundMesh = true;
                        meshSource = "MeshFilter";
                    }
                    else {
                        meshFilter = go.GetComponentInChildren<MeshFilter>();

                        if (meshFilter != null && meshFilter.sharedMesh != null) {
                            bounds = meshFilter.sharedMesh.bounds;
                            foundMesh = true;
                            meshSource = "MeshFilter (child)";
                        }
                    }
                }
            }

            if (!foundMesh) {
                if (!SuppressConsole) {
                    Debug.LogWarning($"No mesh found on {go.name} or its children. " +
                                     $"Using fallback values for non-overridden values.");
                }

                SetFallbackValues();

                return;
            }

            if (!SuppressConsole)
                Debug.Log($"Using mesh bounds from {meshSource} on {go.name}");

            if (!OverrideHeight) {
                Height = bounds.size.y;
                ValidateAndClampHeight();
            }

            if (!OverrideRadius) {
                var meshRadius = Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f;
                Radius = meshRadius;
                ValidateAndClampRadius();
            }

            CenterY = Height * 0.5f;

            ValidateConfiguration();
        }

        private void SetFallbackValues() {
            if (!OverrideHeight) {
                Height = FallbackHeight;
                ValidateAndClampHeight();
            }

            if (!OverrideRadius) {
                Radius = FallbackRadius;
                ValidateAndClampRadius();
            }

            CenterY = Height * 0.5f;
        }

        private void ValidateAndClampHeight() {
            if (Height <= 0f) {
                Debug.LogWarning($"Calculated height {Height} is invalid. Using fallback height {FallbackHeight}.");
                Height = FallbackHeight;
            }

            // Reasonable limits for character height
            Height = Mathf.Clamp(Height, 0.1f, 10f);
        }

        private void ValidateAndClampRadius() {
            if (Radius <= 0f) {
                Debug.LogWarning($"Calculated radius {Radius} is invalid. Using fallback radius {FallbackRadius}.");
                Radius = FallbackRadius;
            }

            Radius = Mathf.Clamp(Radius, 0.05f, 200f);
        }

        private void ValidateConfiguration() {
            if (CenterY > Height) {
                Debug.LogWarning($"CenterY ({CenterY}) cannot be higher than Height ({Height}). Clamping to height.");
                CenterY = Height;
            }

            if (CenterY < 0f) {
                Debug.LogWarning($"CenterY ({CenterY}) cannot be negative. Setting to 0.");
                CenterY = 0f;
            }

            if (!SuppressConsole)
                Debug.Log(
                    $"DefaultColliderData initialized: Height={Height:F2}, CenterY={CenterY:F2}, Radius={Radius:F2}");
        }

        /// <summary>
        /// Call this when values are changed in inspector to re-validate
        /// </summary>
        public void HandleValidation() {
            if (Height > 0)
                CenterY = Height * 0.5f;

            ValidateAndClampHeight();
            ValidateAndClampRadius();
            ValidateConfiguration();
        }
    }
}