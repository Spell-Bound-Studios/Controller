using SpellBound.Controller.PlayerController;

namespace SpellBound.Controller.PlayerStateMachine {
    /// <summary>
    /// Minimal contract a state must follow. ScriptableObjects in the game project can implement
    /// this by inheriting a base SO (see BaseSoState below) or implementing directly if they prefer.
    /// </summary>
    public interface IState {
        public string Id { get; }
        public string AssetName { get; }
        public void OnEnter(ControllerBase ctx);
        public void OnUpdate();
        public void OnFixedUpdate();
        public void OnExit();
    }
}