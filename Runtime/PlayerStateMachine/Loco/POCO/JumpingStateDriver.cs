namespace SpellBound.Controller.PlayerStateMachine {
    public class JumpingStateDriver  : BaseLocoStateDriver {
        public JumpingStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.JumpingState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.JumpingState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.JumpingState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.JumpingState.ExitStateLogic();
    }
}