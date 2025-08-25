using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public class StateDatabase : MonoBehaviour {
        public static StateDatabase Instance;
        private readonly Dictionary<string, BaseLocoStateSO> _locoStatePresets = new();
        private readonly Dictionary<string, BaseActionStateSO> _actionStatePresets = new();

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        
            var locoPresets = Resources.LoadAll<BaseLocoStateSO>("");
            foreach (var preset in locoPresets) {
                if (!_locoStatePresets.TryAdd(preset.assetName, preset)) {
                    Debug.LogError($"Duplicate uid found {preset.uid}", this);
                }
            }
            
            var actionPresets = Resources.LoadAll<BaseActionStateSO>("");
            foreach (var preset in actionPresets) {
                if (!_actionStatePresets.TryAdd(preset.assetName, preset)) {
                    Debug.LogError($"Duplicate uid found {preset.uid}", this);
                }
            }
        }

        /// <summary>
        /// Attempts to get the preset given a unique ID.
        /// Use whenever you need data for a specific unique ID.
        /// </summary>
        public bool TryGetLocoState(string displayName, out BaseLocoStateSO preset) {
            if (!string.IsNullOrEmpty(displayName)) {
                return _locoStatePresets.TryGetValue(displayName, out preset);
            }
            
            Debug.LogError($"Could not find state with name {displayName}", this);
            
            preset = null;
            return false;
        }
        
        public bool TryGetActionState(string displayName, out BaseActionStateSO preset) {
            if (!string.IsNullOrEmpty(displayName)) {
                return _actionStatePresets.TryGetValue(displayName, out preset);
            }
            
            Debug.LogError($"Could not find state with name {displayName}", this);
            
            preset = null;
            return false;
        }
    }
}