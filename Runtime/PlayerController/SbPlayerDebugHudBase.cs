using System;
using System.Collections.Generic;
using System.ComponentModel;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// This script should only be attached if you're interested in debugging to canvas. It is purely for debugging
    /// purposes and does not use the best practices for performance.
    /// </summary>
    public class SbPlayerDebugHudBase : MonoBehaviour {
        public static SbPlayerDebugHudBase Instance;
        
        [Header("References"), Description("Drag and drop the prefab here"), Tooltip("Drag and drop it!")] 
        [SerializeField] private Canvas debuggingCanvasPrefab;
        
        [Header("HUD Settings")]
        [SerializeField] private DebugHudProfile profile;
        [SerializeField] private List<FieldOption> fieldToggles = new();
        
        [Header("Gizmos")] 
        [SerializeField] private bool showRaycastGizmos = true;
        
        private readonly Dictionary<string, Func<string>> _getters = new();
        private readonly Dictionary<string, TMP_Text> _labels = new();
        private readonly List<Action> _gizmos = new();
        private readonly Dictionary<string, FieldOption> _toggleIndex = new();
        
        private RigidbodyMover _mover;
        private SbCharacterControllerBase _controller;
        private RaycastSensor _sensor;
        private RectTransform _container;

        private bool _togglesDirty;
        private bool _layoutDirty;
        private bool _profileDirty;
        
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if (!profile)
                Debug.LogError("Profile object is missing. Please locate in prefab folder and drag and drop.", 
                        this);
            
            if (!debuggingCanvasPrefab) {
                Debug.LogError("Debug HUD: missing Canvas prefab.", this);
                enabled = false;
                return;
            }
            
            var canvas = Instantiate(debuggingCanvasPrefab);
            _container = EnsureContainer(canvas.transform);
            
            SyncFromProfile();
            RebuildToggleIndex();
        }

        private void OnEnable() {
            _mover = GetComponent<RigidbodyMover>();
            if (_mover)
                _sensor = _mover.GetRaycastSensor();
            
            _controller = GetComponent<SbCharacterControllerBase>();

            RegisterAllProviders();
            ApplyToggleVisibility();
        }

        private void OnDisable() {
            _getters.Clear();
            _labels.Clear();
            _gizmos.Clear();

            if (!_container)
                return;
            
            foreach (Transform child in _container) 
                Destroy(child.gameObject);
        }
        
        private void Update() {
            foreach (var kv in _getters) {
                if (!IsEnabled(kv.Key)) 
                    continue;
                
                if (!_labels.TryGetValue(kv.Key, out var label) || !label) 
                    continue;

                string value;

                try {
                    value = kv.Value?.Invoke() ?? "";
                }
                catch {
                    value = "(err)";
                }
                
                label.text = $"{kv.Key}: {value}";
            }
        }
        
        private void LateUpdate() {
            if (_togglesDirty) {
                _togglesDirty = false;
                ApplyToggleVisibility();
            }

            if (_profileDirty) {
                _profileDirty = false;
                PushToProfile();
            }

            if (!_layoutDirty || !_container) 
                return;

            _layoutDirty = false;
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
            Canvas.ForceUpdateCanvases();
        }

        #region Debugging Backend
        // #########################
        // Debugging Infrastructure.
        // #########################
        
        private void OnDrawGizmosSelected() {
            if (!showRaycastGizmos) 
                return;

            foreach (var t in _gizmos)
                t?.Invoke();
        }
        
        private RectTransform EnsureContainer(Transform canvasRoot) {
            var panel = new GameObject(
                    "DebugHUD",
                    typeof(RectTransform),
                    typeof(Image),
                    typeof(VerticalLayoutGroup),
                    typeof(ContentSizeFitter)
            );
            var rt = (RectTransform)panel.transform;
            rt.SetParent(canvasRoot, false);
            
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(12f, -12f);
            rt.sizeDelta = new Vector2(420f, 0f);
            
            var img = panel.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.45f);
            
            var v = panel.GetComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.UpperLeft;
            v.childForceExpandWidth  = false;
            v.childForceExpandHeight = false;
            v.padding = new RectOffset(8, 8, 8, 8);
            v.spacing = 4f;
            
            var fit = panel.GetComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fit.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            return rt;
        }
        
        private TMP_Text CreateRow(string key) {
            var go = new GameObject($"{key}", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(_container, false);
            
            var le = go.GetComponent<LayoutElement>();
            le.preferredHeight = 22f;
            le.flexibleHeight = 0f;
            le.flexibleWidth = 0f;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 16;
            tmp.alignment = TextAlignmentOptions.TopLeft;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.raycastTarget = false;
            tmp.text = $"{key}:";
            
            var rt = (RectTransform)go.transform;
            rt.sizeDelta = new Vector2(0f, 22f);

            return tmp;
        }

        private void RegisterAllProviders() {
            _getters.Clear();
            _labels.Clear();
            _gizmos.Clear();
            
            foreach (Transform child in _container) 
                Destroy(child.gameObject);
            
            var comps = GetComponents<IDebuggingInfo>();
            foreach (var t in comps)
                t.RegisterDebugInfo(this);
            
            if (_sensor is IDebuggingInfo dbg) 
                dbg.RegisterDebugInfo(this);
        }
        
        public void Field(string key, Func<string> getter) {
            if (string.IsNullOrEmpty(key) || getter == null) return;
            
            _getters[key] = getter;

            EnsureToggle(key);

            EnsureRowVisibility(key, IsEnabled(key));
        }

        public void Gizmo(Action draw) {
            if (draw != null) 
                _gizmos.Add(draw);
        }
        
        private void RebuildToggleIndex() {
            _toggleIndex.Clear();

            foreach (var fo in fieldToggles) {
                if (fo != null && !string.IsNullOrEmpty(fo.key))
                    _toggleIndex[fo.key] = fo;
            }
        }
        
        private void EnsureToggle(string key) {
            if (_toggleIndex.ContainsKey(key)) 
                return;

            var opt = new FieldOption {
                    key = key, 
                    enabled = true
            };
            
            fieldToggles.Add(opt);
            _toggleIndex[key] = opt;

            if (!profile) 
                return;

            profile.GetOrAdd(key);
#if UNITY_EDITOR
            _profileDirty = true;
#endif
        }

        private bool IsEnabled(string key) {
            return !_toggleIndex.TryGetValue(key, out var opt) || opt.enabled;
        }

        private void EnsureRowVisibility(string key, bool shouldExist) {
            var has = _labels.TryGetValue(key, out var label) && label;

            switch (shouldExist) {
                case true when !has:
                    _labels[key] = CreateRow(key);
                    _layoutDirty = true;
                    break;
                case false when has:
                    Destroy(label.gameObject);
                    _labels.Remove(key);
                    _layoutDirty = true;
                    break;
            }
        }

        private void ApplyToggleVisibility() {
            RebuildToggleIndex();
            
            foreach (var key in _getters.Keys)
                EnsureRowVisibility(key, IsEnabled(key));
            
            _layoutDirty = true;
        }
        
        private void SyncFromProfile() {
            if (!profile) 
                return;

            var existing = new Dictionary<string, FieldOption>(fieldToggles.Count);
            
            foreach (var fo in fieldToggles) 
                if (fo != null && !string.IsNullOrEmpty(fo.key)) 
                    existing[fo.key] = fo;

            foreach (var kv in profile.Index) {
                if (!existing.TryGetValue(kv.Key, out var value))
                    fieldToggles.Add(new FieldOption { key = kv.Key, enabled = kv.Value.enabled });
                else
                    value.enabled = kv.Value.enabled;
            }
            RebuildToggleIndex();
        }
        
        private void PushToProfile() {
            if (!profile) 
                return;

            foreach (var fo in fieldToggles) {
                if (fo == null || string.IsNullOrEmpty(fo.key)) 
                    continue;
                
                var p = profile.GetOrAdd(fo.key);
                p.enabled = fo.enabled;
            }
#if UNITY_EDITOR
            profile.SaveNow();
#endif
        }
        
#if UNITY_EDITOR
        private void OnValidate() {
            if (!Application.isPlaying) 
                return;
            
            _togglesDirty = true;
            _profileDirty = true;
        }
#endif

        [Serializable]
        private class FieldOption {
            public string key;
            public bool enabled = true;
        }
        
        #endregion
    }
}