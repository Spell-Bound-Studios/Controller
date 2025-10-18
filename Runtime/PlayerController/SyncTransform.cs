// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Place this on a gameobject that you want to 1:1 match the transform with some offset.
    /// </summary>
    public class SyncTransform : MonoBehaviour {
        public static SyncTransform Instance;

        [SerializeField] private Transform followThisTransform;
        [SerializeField] private Vector3 offset = new(0f, 2f, 0f);
        private Transform _tr;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);

                return;
            }

            Instance = this;

            _tr = transform;

            if (!followThisTransform)
                enabled = false;
        }

        private void OnDisable() => followThisTransform = null;

        private void LateUpdate() {
            if (!followThisTransform) {
                enabled = false;

                return;
            }

            _tr.position = followThisTransform.position + offset;
        }

        public void SetFollowTransform(Transform tr) {
            followThisTransform = tr;
            enabled = true;
        }
    }
}