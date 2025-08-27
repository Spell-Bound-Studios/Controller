namespace SpellBound.Controller.PlayerStateMachine {
    public class GCDStateDriver : BaseActionStateDriver {
        public GCDStateDriver(ActionStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.GCDState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.GCDState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.GCDState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.GCDState.ExitStateLogic();
    }
}