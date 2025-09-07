namespace SpellBound.Controller.PlayerStateMachine {
    public class SlidingStateDriver : BaseLocoStateDriver {
        public SlidingStateDriver(LocoStateMachine stateMachine) : base(stateMachine) { }
        public override void EnterState() => StateMachine.SlidingState.EnterStateLogic(StateMachine);
        public override void UpdateState() => StateMachine.SlidingState.UpdateStateLogic();
        public override void FixedUpdateState() => StateMachine.SlidingState.FixedUpdateStateLogic();
        public override void ExitState() => StateMachine.SlidingState.ExitStateLogic();
    }
}