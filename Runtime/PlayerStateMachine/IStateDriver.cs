using SpellBound.Controller.PlayerController;

namespace SpellBound.Controller.PlayerStateMachine {
    /// <summary>
    /// Optional adapter if you want to keep the "Driver" concept. Most games won't need custom drivers
    /// if states are ScriptableObjects, but the hook exists for advanced cases (split logic, caching, etc.).
    /// </summary>
    public interface IStateDriver {
        public string Id { get; }
        public IState State { get; }
        public void Enter(ControllerBase ctx);
        public void Update();
        public void FixedUpdate();
        public void Exit();
    }
}