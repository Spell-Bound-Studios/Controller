namespace SpellBound.Controller.PlayerStateMachine {
    public class ReadyStateSO : BaseActionStateSO {
        public override void EnterStateLogic(ActionStateMachine stateMachine) {
            StateMachine = stateMachine;
            StateHelper.NotifyActionStateChange(this);
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            Cc.HandleHorizontalVelocityInput();
        }
        
        public override void CheckSwitchStateLogic() {

        }
        
        public override void ExitStateLogic() {
            
        }
    }
}