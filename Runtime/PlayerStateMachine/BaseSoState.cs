using System;
using SpellBound.Controller.PlayerController;
using SpellBound.Core;
using Unity.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    /// <summary>
    /// An abstract base ScriptableObject that implements IState. This lives in the Controller, so games can inherit it,
    /// but it contains no game logic. (Keep UnityEditor bits behind #if UNITY_EDITOR as you have done.)
    /// </summary>
    public abstract class BaseSoState : ScriptableObject, IState {
        [SerializeField, ReadOnly, Immutable] private string id;
        [SerializeField, ReadOnly, Immutable] private string assetName;

        protected ControllerBase Ctx { get; private set; }
        public string Id => id;
        public string AssetName => assetName;
        
#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on an asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var newName = name;
            
            if (assetPath == null) {
                id = string.Empty;
                return;
            }

            if (assetName != newName) {
                assetName = newName;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            
            var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (string.IsNullOrEmpty(id) || id != assetGuid) {
                id = assetGuid;
            }
            
            var interfaces = GetType().GetInterfaces();
            var hasStateType = false;
            foreach (var state in interfaces) {
                if (!state.IsGenericType || state.GetGenericTypeDefinition() != typeof(IStateType<>)) 
                    continue;

                hasStateType = true;
                break;
            }
            if (!hasStateType) {
                Debug.LogError($"{name}: state must implement IStateType<TEnum> and expose a serialized enum field.", 
                        this);
            }
        }
#endif

        public void OnEnter(ControllerBase ctx) {
            Ctx = ctx; 
            EnterStateLogic();
        }
        
        public void OnUpdate() => UpdateStateLogic();
        public void OnFixedUpdate() => FixedUpdateStateLogic();
        public void OnExit() => ExitStateLogic();

        /// <summary>
        /// Inheritors must override and implement their own logic.
        /// </summary>
        protected abstract void EnterStateLogic();
        protected abstract void UpdateStateLogic();
        protected abstract void FixedUpdateStateLogic();
        protected abstract void ExitStateLogic();
    }
}