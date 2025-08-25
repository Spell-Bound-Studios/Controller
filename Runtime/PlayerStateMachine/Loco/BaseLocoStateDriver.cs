namespace SpellBound.Controller.PlayerStateMachine {
    public abstract class BaseLocoStateDriver {
        protected readonly LocoStateMachine StateMachine;
        protected BaseLocoStateDriver(LocoStateMachine locoStateMachine) => StateMachine = locoStateMachine;
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
    }
}