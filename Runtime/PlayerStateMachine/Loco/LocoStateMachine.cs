namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class LocoStateMachine {
        internal LocoStateContext Ctx;
        
        public BaseLocoStateDriver CurrentLocoStateDriver;
        
        public GroundStateDriver GroundStateDriver;
        public GroundStateSO GroundState;
        
        public LocoStateMachine() {
            GroundStateDriver = new GroundStateDriver(this);
            
            CurrentLocoStateDriver = GroundStateDriver;
            CurrentLocoStateDriver.EnterState();
        }

        public void SetContext(in LocoStateContext context) {
            Ctx = context;
        }
    }
}