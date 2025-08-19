namespace SpellBound.Controller.PlayerStateMachine {
    public abstract class BaseActionState {
        protected ActionStateMachine StateMachine;

        protected BaseActionState(ActionStateMachine actionStateMachine) {
            StateMachine = actionStateMachine;
        }
        
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void FixedUpdateState();
        public abstract void ExitState();
    }
}