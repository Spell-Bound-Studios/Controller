namespace SpellBound.Controller.PlayerStateMachine {
    public class JumpingStateDriver  : BaseLocoStateDriver {
        public JumpingStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.JumpingState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.JumpingState.UpdateStateLogic(in StateMachine.Ctx);
        public override void FixedUpdateState() => StateMachine.JumpingState.FixedUpdateStateLogic(in StateMachine.Ctx);
        public override void ExitState() => StateMachine.JumpingState.ExitStateLogic();
    }
}