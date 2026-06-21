// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Runtime UIToolkit panel showcasing the camera API: switch the live camera by <see cref="CameraProfile"/>
    /// (discovered from <see cref="CameraProfileRegistry"/>) and drive the live <see cref="ICameraSettings"/>
    /// (sensitivity, invert, smoothing, pitch) with sliders/toggles. Demo scaffolding.
    ///
    /// Setup: put this on a GameObject; it adds a <see cref="UIDocument"/>. Assign a Panel Settings and LEAVE the
    /// Source Asset empty — the panel is built in code. Press the toggle key to free the cursor so you can click.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class CameraDemoPanelExample : MonoBehaviour {
        [SerializeField,
         Tooltip("Controller whose CameraData (ICameraSettings) the sliders drive. Auto-found if left empty.")]
        private PlayerControllerExample controller;

        [SerializeField,
         Tooltip("Key that frees the cursor and pauses look so you can click the panel; press again to resume.")]
        private Key toggleCursorKey = Key.Tab;

        private readonly List<(Button button, CameraProfile profile)> _camButtons = new();
        private UIDocument _document;
        private VisualElement _panel;
        private VisualElement _content;
        private Label _status;
        private bool _populated;
        private bool _cursorFree;

        private void Awake() {
            _document = GetComponent<UIDocument>();

            if (_document.panelSettings == null) {
                _document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                Log.Warn("CameraDemoPanelExample created a runtime PanelSettings; assign one for reliable styling.");
            }
        }

        private void Update() {
            EnsurePanelAttached();

            if (_panel == null)
                return;

            if (Keyboard.current != null && Keyboard.current[toggleCursorKey].wasPressedThisFrame)
                ToggleCursor();

            if (controller == null) {
                controller = FindAnyObjectByType<PlayerControllerExample>();
                _populated = false;
            }

            if (controller != null && CameraRigManager.Instance != null) {
                if (!_populated)
                    Populate();

                RefreshHighlights();
            }
            else if (_populated) {
                _content.Clear();
                _camButtons.Clear();
                _populated = false;
            }

            UpdateStatus();
        }

        private void EnsurePanelAttached() {
            var liveRoot = _document.rootVisualElement;

            if (liveRoot == null || (_panel != null && _panel.parent == liveRoot))
                return;

            BuildChrome();
            _populated = false;
        }

        private void BuildChrome() {
            var root = _document.rootVisualElement;
            root.Clear();

            _panel = new VisualElement {
                style = {
                    position = Position.Absolute, top = 10f, left = 10f, width = 260f,
                    paddingTop = 8f, paddingBottom = 8f, paddingLeft = 8f, paddingRight = 8f,
                    backgroundColor = new Color(0f, 0f, 0f, 0.8f)
                }
            };
            root.Add(_panel);

            _panel.Add(MakeLabel("Camera API", 14f, Color.white, true));
            _status = MakeLabel(string.Empty, 10f, new Color(0.85f, 0.85f, 0.55f), false);
            _panel.Add(_status);

            _content = new VisualElement();
            _panel.Add(_content);
        }

        private void Populate() {
            _content.Clear();
            _camButtons.Clear();

            _content.Add(MakeLabel("Camera", 12f, new Color(0.6f, 0.8f, 1f), false));

            var row = new VisualElement {
                style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6f }
            };
            _content.Add(row);

            foreach (var profile in CameraProfileRegistry.All) {
                if (profile == null)
                    continue;

                var captured = profile;
                var button = new Button(() => Switch(captured)) { text = captured.name };
                StyleButton(button);
                row.Add(button);
                _camButtons.Add((button, captured));
            }

            _content.Add(MakeLabel("Look", 12f, new Color(0.6f, 0.8f, 1f), false));

            var s = controller.CameraData;
            _content.Add(MakeSlider("Sensitivity X", 0f, 5f, s.SensitivityX, v => s.SensitivityX = v));
            _content.Add(MakeSlider("Sensitivity Y", 0f, 5f, s.SensitivityY, v => s.SensitivityY = v));
            _content.Add(MakeToggle("Invert Y", s.InvertY, v => s.InvertY = v));
            _content.Add(MakeToggle("Smooth Look", s.SmoothLook, v => s.SmoothLook = v));
            _content.Add(MakeSlider("Smoothing", 1f, 50f, s.SmoothingFactor, v => s.SmoothingFactor = v));
            _content.Add(MakeSlider("Min Pitch", -89f, 0f, s.MinPitch, v => s.MinPitch = v));
            _content.Add(MakeSlider("Max Pitch", 0f, 89f, s.MaxPitch, v => s.MaxPitch = v));

            _populated = true;
        }

        private void Switch(CameraProfile profile) {
            CameraRigManager.Instance.Switch(profile);
            RefreshHighlights();
        }

        private void RefreshHighlights() {
            var current = CameraRigManager.Instance.Current;

            foreach (var (button, profile) in _camButtons)
                button.style.backgroundColor = profile == current
                        ? new Color(0.18f, 0.5f, 0.9f)
                        : new Color(0.22f, 0.22f, 0.26f);
        }

        private void ToggleCursor() {
            _cursorFree = !_cursorFree;
            Cursor.lockState = _cursorFree
                    ? CursorLockMode.None
                    : CursorLockMode.Locked;
            Cursor.visible = _cursorFree;

            if (controller != null)
                controller.SetCameraFollowMouse(!_cursorFree);
        }

        private void UpdateStatus() {
            if (_status == null)
                return;

            if (controller == null || CameraRigManager.Instance == null)
                _status.text = "Waiting for a spawned character…";
            else
                _status.text = _cursorFree
                        ? $"Cursor free — adjust + click.  [{toggleCursorKey}] resumes."
                        : $"[{toggleCursorKey}] frees cursor to click.";
        }

        private static Label MakeLabel(string text, float size, Color color, bool header) =>
                new(text) {
                    style = {
                        fontSize = size, color = color,
                        unityFontStyleAndWeight = header
                                ? FontStyle.Bold
                                : FontStyle.Normal,
                        marginTop = header
                                ? 0f
                                : 4f,
                        marginBottom = 2f
                    }
                };

        private static Slider MakeSlider(string label, float min, float max, float value, Action<float> onChange) {
            var slider = new Slider(label, min, max) { value = value };
            slider.style.marginBottom = 2f;
            slider.labelElement.style.color = Color.white;
            slider.labelElement.style.minWidth = 90f;
            slider.RegisterValueChangedCallback(evt => onChange(evt.newValue));

            return slider;
        }

        private static Toggle MakeToggle(string label, bool value, Action<bool> onChange) {
            var toggle = new Toggle(label) { value = value };
            toggle.style.marginBottom = 2f;
            toggle.labelElement.style.color = Color.white;
            toggle.labelElement.style.minWidth = 90f;
            toggle.RegisterValueChangedCallback(evt => onChange(evt.newValue));

            return toggle;
        }

        private static void StyleButton(Button button) {
            button.style.marginRight = 4f;
            button.style.marginTop = 2f;
            button.style.paddingLeft = 6f;
            button.style.paddingRight = 6f;
            button.style.color = Color.white;
            button.style.fontSize = 11f;
        }
    }
}
