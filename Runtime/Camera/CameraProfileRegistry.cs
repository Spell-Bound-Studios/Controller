// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Registries;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Resolves a <see cref="CameraProfile"/> from its stable uint hash — "send one id, get the camera role." A
    /// thin facade over a Core <see cref="HashRegistry{TEntry}"/> populated once from every profile asset under a
    /// <c>Resources/CameraProfiles</c> folder (same pattern as <see cref="StateRegistry"/>).
    /// </summary>
    public static class CameraProfileRegistry {
        private const string ResourceFolder = "CameraProfiles";

        private static readonly HashRegistry<CameraProfile> Registry = new();
        private static bool _isLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void WarmUp() => EnsureLoaded();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlaySession() {
            Registry.Clear();
            _isLoaded = false;
        }

        public static int Count {
            get {
                EnsureLoaded();

                return Registry.Count;
            }
        }

        public static IReadOnlyList<CameraProfile> All {
            get {
                EnsureLoaded();

                return Registry.All;
            }
        }

        public static bool TryGet(uint hash, out CameraProfile profile) {
            EnsureLoaded();

            return Registry.TryGet(hash, out profile);
        }

        public static CameraProfile Get(uint hash) {
            EnsureLoaded();

            return Registry.Get(hash);
        }

        public static bool Contains(uint hash) {
            EnsureLoaded();

            return Registry.Contains(hash);
        }

        public static void Write(ref Span<byte> buffer, CameraProfile profile) =>
                RegistryPacker.Write(ref buffer, profile);

        public static CameraProfile Read(ref ReadOnlySpan<byte> buffer) {
            EnsureLoaded();

            return RegistryPacker.Read(ref buffer, Registry);
        }

        private static void EnsureLoaded() {
            if (_isLoaded)
                return;

            // Set first so the scan runs once even if the CameraProfiles folder is missing or empty.
            _isLoaded = true;
            Registry.AddRange(Resources.LoadAll<CameraProfile>(ResourceFolder));
        }
    }
}
