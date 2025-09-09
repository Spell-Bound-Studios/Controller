using SpellBound.Controller.PlayerController;
using Unity.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    /// <summary>
    /// An abstract base ScriptableObject that implements IState. This lives in the Controller, so games can inherit it,
    /// but it contains no game logic. (Keep UnityEditor bits behind #if UNITY_EDITOR as you have done.)
    /// </summary>
    public abstract class BaseSoState : ScriptableObject, IState {
        [SerializeField, ReadOnly] private string id;
        [SerializeField, ReadOnly] private string assetName;
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