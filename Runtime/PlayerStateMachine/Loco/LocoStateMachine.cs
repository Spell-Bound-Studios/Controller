using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class LocoStateMachine {
        public PlayerController.PlayerController PlayerController;
        public BaseLocoStateDriver CurrentLocoStateDriver;
        
        public GroundStateDriver GroundStateDriver;
        public GroundStateSO GroundState;
        
        public FallingStateDriver FallingStateDriver;
        public FallingStateSO FallingState;
        
        public LandingStateDriver LandingStateDriver;
        public LandingStateSO LandingState;
        
        public JumpingStateDriver JumpingStateDriver;
        public JumpingStateSO JumpingState;
        
        internal LocoStateContext Ctx;
        
        public LocoStateMachine(PlayerController.PlayerController pc, List<string> defaultStatesList) {
            PlayerController = pc;
            
            GroundStateDriver = new GroundStateDriver(this);
            FallingStateDriver = new FallingStateDriver(this);
            LandingStateDriver = new LandingStateDriver(this);
            JumpingStateDriver =  new JumpingStateDriver(this);
            
            var defaultStates = StateHelper.GetDefaultStatesFromDB(defaultStatesList);

            foreach (var state in defaultStates) {
                switch (state) {
                    case GroundStateSO gso:
                        GroundState = gso;
                        break;
                    case FallingStateSO fso:
                        FallingState = fso;
                        break;
                    case LandingStateSO lso:
                        LandingState = lso;
                        break;
                    case JumpingStateSO jso:
                        JumpingState = jso;
                        break;
                }
            }
            
            if (defaultStates.Count <= 0)
                Debug.LogError($"Default state count found in constructor: {defaultStates.Count}. Please verify that" +
                                "default states list is created/exists by instantiator.");

            if (CurrentLocoStateDriver != null) 
                return;

            CurrentLocoStateDriver = FallingStateDriver;
            CurrentLocoStateDriver.EnterState();
        }

        public void SetContext(in LocoStateContext context) {
            Ctx = context;
        }

        public void ChangeState(BaseLocoStateDriver newDriver) {
            CurrentLocoStateDriver.ExitState();
            CurrentLocoStateDriver = newDriver;
            CurrentLocoStateDriver.EnterState();
        }
    }
}