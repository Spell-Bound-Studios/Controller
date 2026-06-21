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
    /// Runtime UIToolkit panel that lists each locomotion slot and the state variants registered for it (found in
    /// <see cref="StateRegistry"/>) and applies a chosen one to the live machine via ApplyVariant, so you can swap
    /// states and feel them without leaving play mode.
    ///
    /// Setup: put this on a GameObject; it adds a <see cref="UIDocument"/>. Assign a Panel Settings asset and
    /// LEAVE the Source Asset empty — the panel is built entirely in code. The character is spawned at runtime, so
    /// the panel waits for it and auto-binds. Because spawning locks the cursor for mouse-look, press the toggle
    /// key to free the cursor (and pause look) so you can click. Demo scaffolding, not part of the controller.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class VariantSwitcherExample : MonoBehaviour {
        [SerializeField,
         Tooltip("Controller whose loco state machine this panel swaps variants on. Auto-found (incl. the spawned " +
                 "instance) when left empty.")]
        private PlayerControllerExample controller;

        [SerializeField,
         Tooltip("Key that frees the cursor and pauses mouse-look so you can click the panel; press again to resume.")]
        private Key toggleCursorKey = Key.Tab;

        private readonly List<(Button button, LocoStateTypes slot, Type type)> _buttons = new();
        private UIDocument _document;
        private VisualElement _slotContainer;
        private Label _status;
        private VisualElement _panel;
        private bool _populated;
        private bool _cursorFree;

        private void Awake() {
            _document = GetComponent<UIDocument>();

            if (_document.panelSettings == null) {
                _document.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
                Log.Warn("VariantSwitcherExample created a runtime PanelSettings; assign one for reliable styling.");
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

            if (controller != null && controller.locoStateMachine != null) {
                if (!_populated)
                    PopulateVariants();

                RefreshHighlights();
            }
            else if (_populated) {
                _slotContainer.Clear();
                _buttons.Clear();
                _populated = false;
            }

            UpdateStatus();
        }

        // UIDocument can clear or recreate its rootVisualElement after startup (which silently drops a code-built
        // panel — the "flash then gone" symptom); rebuild whenever ours is no longer attached to the live root.
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
                    position = Position.Absolute, top = 10f, right = 10f, width = 240f,
                    paddingTop = 8f, paddingBottom = 8f, paddingLeft = 8f, paddingRight = 8f,
                    backgroundColor = new Color(0f, 0f, 0f, 0.8f)
                }
            };
            root.Add(_panel);

            _panel.Add(MakeLabel("State Variants", 14f, Color.white, true));
            _status = MakeLabel(string.Empty, 10f, new Color(0.85f, 0.85f, 0.55f), false);
            _panel.Add(_status);

            _slotContainer = new VisualElement();
            _panel.Add(_slotContainer);
        }

        /// <summary>
        /// The variant filling each slot when the character appears defines that slot's type family; every
        /// registered state assignable to it becomes a swap option for the slot.
        /// </summary>
        private void PopulateVariants() {
            _slotContainer.Clear();
            _buttons.Clear();

            foreach (LocoStateTypes slot in Enum.GetValues(typeof(LocoStateTypes))) {
                var current = controller.locoStateMachine.GetCurrentVariant(slot);

                if (current == null)
                    continue;

                var slotType = current.GetType();
                _slotContainer.Add(MakeLabel(slot.ToString(), 12f, new Color(0.6f, 0.8f, 1f), false));

                var row = new VisualElement {
                    style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginBottom = 6f }
                };
                _slotContainer.Add(row);

                foreach (var state in StateRegistry.All) {
                    if (state == null || !slotType.IsInstanceOfType(state))
                        continue;

                    var variant = state;
                    var button = new Button(() => ApplyVariant(slot, variant)) { text = variant.name };
                    StyleButton(button);
                    row.Add(button);
                    _buttons.Add((button, slot, variant.GetType()));
                }
            }

            _populated = true;
        }

        private void ApplyVariant(LocoStateTypes slot, BaseSoState variant) {
            controller.locoStateMachine.ApplyVariant(slot, variant);
            RefreshHighlights();
        }

        private void RefreshHighlights() {
            foreach (var (button, slot, type) in _buttons) {
                var current = controller.locoStateMachine.GetCurrentVariant(slot);

                button.style.backgroundColor = current != null && current.GetType() == type
                        ? new Color(0.18f, 0.5f, 0.9f)
                        : new Color(0.22f, 0.22f, 0.26f);
            }
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

            if (controller == null || controller.locoStateMachine == null)
                _status.text = "Waiting for a spawned character…";
            else if (_buttons.Count == 0)
                _status.text = "No states found under Resources/States.";
            else
                _status.text = _cursorFree
                        ? $"Cursor free — click to swap.  [{toggleCursorKey}] resumes."
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
