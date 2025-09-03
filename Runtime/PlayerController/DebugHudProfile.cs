using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    [CreateAssetMenu(fileName = "DebugHUDProfile", menuName = "Spellbound/CharacterController/DebugHUDProfile")]
    public class DebugHudProfile : ScriptableObject {
        [Serializable]
        public class FieldOption {
            public string key; 
            public bool enabled = true;
        }
        
        public List<FieldOption> fieldToggles = new();

        [NonSerialized] private Dictionary<string, FieldOption> _index;

        public Dictionary<string, FieldOption> Index {
            get {
                if (_index != null) 
                    return _index;

                _index = new Dictionary<string, FieldOption>(fieldToggles.Count);
                foreach (var fo in fieldToggles)
                    if (fo != null && !string.IsNullOrEmpty(fo.key))
                        _index[fo.key] = fo;
                return _index;
            }
        }

        public FieldOption GetOrAdd(string key) {
            var idx = Index;

            if (idx.TryGetValue(key, out var fo)) 
                return fo;

            fo = new FieldOption {
                    key = key, 
                    enabled = true
            };
            
            fieldToggles.Add(fo);
            idx[key] = fo;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
            return fo;
        }

#if UNITY_EDITOR
        public void SaveNow() {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}