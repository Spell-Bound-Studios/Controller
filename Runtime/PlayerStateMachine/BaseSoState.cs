﻿using SpellBound.Core;
using Unity.Collections;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// An abstract base ScriptableObject that implements IState. This lives in the Controller, so games can inherit it,
    /// but it contains no game logic. (Keep UnityEditor bits behind #if UNITY_EDITOR as you have done.)
    /// </summary>
    public abstract class BaseSoState : ScriptableObject {
        [SerializeField, ReadOnly, Immutable] private string id;
        [SerializeField, ReadOnly, Immutable] private string assetName;
        
        public string Id => id;
        public string AssetName => assetName;
        public object Ctx;
        
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
        }
#endif

        /// <summary>
        /// Called once when the state machine initializes to cache the context.
        /// </summary>
        public virtual void InitializeWithContext(object ctx) {
            Ctx = ctx;
            OnCtxInitialized();
        }
        
        /// <summary>
        /// Override this to handle context caching setup (like casting to your specific context type).
        /// </summary>
        protected virtual void OnCtxInitialized() { }
        
        public void OnEnter() => EnterStateLogic();
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