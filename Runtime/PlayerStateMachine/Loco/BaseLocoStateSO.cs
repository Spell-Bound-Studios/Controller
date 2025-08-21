using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public abstract class BaseLocoStateSO : ScriptableObject {
        protected LocoStateMachine StateMachine;
        public string uid;
        public string assetName;
        
#if UNITY_EDITOR
        /// <summary>
        /// Creates guids based on an asset path for us when something gets updated.
        /// </summary>
        private void OnValidate() {
            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(this);
            var newName = name;
            
            if (assetPath == null) {
                uid = string.Empty;
                return;
            }

            if (assetName != newName) {
                assetName = newName;
                UnityEditor.EditorUtility.SetDirty(this);
            }
            
            var assetGuid = UnityEditor.AssetDatabase.GUIDFromAssetPath(assetPath).ToString();
            if (string.IsNullOrEmpty(uid) || uid != assetGuid) {
                uid = assetGuid;
            }
        }
#endif

        /// <summary>
        /// This method is called when the state is first entered.
        /// </summary>
        public abstract void EnterStateLogic(LocoStateMachine stateMachine);

        /// <summary>
        /// This method is called every frame while the state is active.
        /// </summary>
        public abstract void UpdateStateLogic(in LocoStateContext ctx);

        /// <summary>
        /// This method is called every fixed frame rate frame while the state is active.
        /// </summary>
        public abstract void FixedUpdateStateLogic(in LocoStateContext ctx);

        /// <summary>
        /// This method checks if the state should transition to another state and
        /// is called in EnterStateLogic in the specific SO.
        /// Think of this as a "What rips the player out of X state".
        /// i.e. an interrupt, movement, etc.
        /// </summary>
        public abstract void CheckSwitchStateLogic(in LocoStateContext ctx);

        /// <summary>
        /// This method is called when the state is exited.
        /// </summary>
        public abstract void ExitStateLogic();
    }
}