namespace SpellBound.Controller.PlayerStateMachine {
    public class ReadyStateDriver : BaseActionStateDriver {
        public ReadyStateDriver(ActionStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.ReadyState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.ReadyState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.ReadyState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.ReadyState.ExitStateLogic();
    }
}