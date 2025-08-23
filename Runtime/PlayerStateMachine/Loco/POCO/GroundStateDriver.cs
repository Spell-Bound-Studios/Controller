namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class GroundStateDriver : BaseLocoStateDriver {
        public GroundStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.GroundState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.GroundState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.GroundState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.GroundState.ExitStateLogic();
    }
}