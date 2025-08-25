namespace SpellBound.Controller.PlayerStateMachine {
    public class LandingStateDriver  : BaseLocoStateDriver {
        public LandingStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.LandingState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.LandingState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.LandingState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.LandingState.ExitStateLogic();
    }
}