namespace SpellBound.Controller.PlayerStateMachine {
    public class FallingStateDriver : BaseLocoStateDriver {
        public FallingStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.FallingState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.FallingState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.FallingState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.FallingState.ExitStateLogic();
    }
}