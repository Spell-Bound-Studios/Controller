namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class GroundStateDriver : BaseLocoStateDriver {
        public GroundStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.GroundState.EnterStateLogic();
        public override void UpdateState() => StateMachine.GroundState.UpdateStateLogic(in StateMachine.Ctx);
        public override void FixedUpdateState() => StateMachine.GroundState.FixedUpdateStateLogic(in StateMachine.Ctx);
        public override void ExitState() => StateMachine.GroundState.ExitStateLogic();
    }
}