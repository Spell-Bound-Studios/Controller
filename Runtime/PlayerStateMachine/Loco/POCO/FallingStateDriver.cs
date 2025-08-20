namespace SpellBound.Controller.PlayerStateMachine {
    public class FallingStateDriver : BaseLocoStateDriver {
        public FallingStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.FallingState.EnterStateLogic();
        public override void UpdateState() => StateMachine.FallingState.UpdateStateLogic(in StateMachine.Ctx);
        public override void FixedUpdateState() => StateMachine.FallingState.FixedUpdateStateLogic(in StateMachine.Ctx);
        public override void ExitState() => StateMachine.FallingState.ExitStateLogic();
    }
}