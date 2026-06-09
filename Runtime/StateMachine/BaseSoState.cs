// Copyright 2025 Spellbound Studio Inc.

using Spellbound.Core.Hashing;
using Spellbound.Core.Registries;
using Spellbound.Core.Tooling;
using Unity.Collections;
using UnityEngine;

namespace Spellbound.Controller {
    /// <summary>
    /// Base type for ScriptableObject states. Each concrete state is an asset that the state machine holds and
    /// swaps. Carries a stable identity — <see cref="Id"/> (asset GUID) and <see cref="Hash"/> (its FNV-1a, the
    /// registry / save / network id) — so a state can be referenced by a single value. See
    /// <see cref="StateRegistry"/>.
    /// </summary>
    public abstract class BaseSoState : ScriptableObject, IRegistryEntry {
        [SerializeField, ReadOnly, Immutable] private string id;
        [SerializeField, ReadOnly, Immutable] private uint hash;
        [SerializeField, ReadOnly, Immutable] private string assetName;
        public string Id => id;
        public uint Hash => hash;
        public string AssetName => assetName;
        public object Ctx;

#if UNITY_EDITOR
        /// <summary>
        /// Keeps <see cref="Id"/> (asset GUID) and <see cref="Hash"/> (its stable FNV-1a) in sync with the asset
        /// whenever it changes. Hashing happens here at edit time only — never at runtime.
        /// </summary>
        private void OnValidate() {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);

            if (string.IsNullOrEmpty(assetPath)) {
                if (!string.IsNullOrEmpty(id) || hash != 0u) {
                    id = string.Empty;
                    hash = 0u;
                    UnityEditor.EditorUtility.SetDirty(this);
                }

                return;
            }

            var newName = name;

            if (assetName != newName) {
                assetName = newName;
                UnityEditor.EditorUtility.SetDirty(this);
            }

            var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            var newHash = StableHash.Fnv1A32(assetGuid);

            if (id != assetGuid || hash != newHash) {
                id = assetGuid;
                hash = newHash;
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }
#endif

        /// <summary>
        /// Called once when the state machine initializes to cache the context.
        /// </summary>
        public virtual void InitializeWithContext(object ctx) {
            Ctx = ctx;
            OnCtxInitialized();
            OnStateInitialize();
        }

        /// <summary>
        /// Override this to handle context caching setup (like casting to your specific context type).
        /// </summary>
        protected virtual void OnCtxInitialized() { }

        protected virtual void OnStateInitialize() { }

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
