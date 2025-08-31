namespace SpellBound.Controller.PlayerStateMachine {
    public class InteractStateDriver : BaseActionStateDriver {
        public InteractStateDriver(ActionStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.InteractState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.InteractState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.InteractState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.InteractState.ExitStateLogic();
    }
}