using SpellBound.Controller.PlayerController;

namespace SpellBound.Controller.PlayerStateMachine {
    /// <summary>
    /// Simple default driver that forwards to an IState. Games get this for free so they
    /// don't have to write drivers per state unless they want to.
    /// </summary>
    public sealed class ForwardingStateDriver : IStateDriver {
        public string Id => State.Id;
        public string AssetName => State.AssetName;
        public IState State { get; }
        private ControllerBase _ctx;

        public ForwardingStateDriver(IState state) => State = state;

        public void Enter(ControllerBase ctx) {
            _ctx = ctx;
            State.OnEnter(_ctx);
        }

        public void Update() => State.OnUpdate();
        public void FixedUpdate() => State.OnFixedUpdate();
        public void Exit() => State.OnExit();
    }
}