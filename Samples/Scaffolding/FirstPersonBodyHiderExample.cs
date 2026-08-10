// Copyright 2026 Spellbound Studio Inc.

using Spellbound.Core.Tooling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Spellbound.Controller.Samples {
    public sealed class FirstPersonBodyHiderExample : MonoBehaviour {
        [SerializeField] private string firstPersonCameraName = "FirstPersonCamera";
        [SerializeField] private CharacterVisualSwitcherExample visualSwitcher;

        private ICameraRig _rig;
        private bool _hidden;
        private string _appliedVisual;

        private void Awake() {
            if (visualSwitcher == null)
                visualSwitcher = GetComponent<CharacterVisualSwitcherExample>();
        }

        private void Update() {
            if (_rig == null && !SingletonManager.TryGetSingletonInstance(out _rig))
                return;

            var hide = _rig.Current == firstPersonCameraName;
            var visual = visualSwitcher != null
                    ? visualSwitcher.Current
                    : null;

            if (hide == _hidden && visual == _appliedVisual)
                return;

            _hidden = hide;
            _appliedVisual = visual;

            var renderers = GetComponentsInChildren<Renderer>(true);

            foreach (var bodyRenderer in renderers)
                bodyRenderer.shadowCastingMode = hide
                        ? ShadowCastingMode.ShadowsOnly
                        : ShadowCastingMode.On;
        }
    }
}
