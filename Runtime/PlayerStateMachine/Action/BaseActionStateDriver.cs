namespace SpellBound.Controller.PlayerStateMachine {
    public abstract class BaseActionStateDriver {
        protected readonly ActionStateMachine StateMachine;
        protected BaseActionStateDriver(ActionStateMachine actionStateMachine) => StateMachine = actionStateMachine;
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
    }
}