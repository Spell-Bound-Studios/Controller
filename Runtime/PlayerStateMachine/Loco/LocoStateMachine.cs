using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class LocoStateMachine {
        internal LocoStateContext Ctx;
        
        public BaseLocoStateDriver CurrentLocoStateDriver;
        
        public GroundStateDriver GroundStateDriver;
        public GroundStateSO GroundState;
        
        public LocoStateMachine(List<string> defaultStatesList) {
            GroundStateDriver = new GroundStateDriver(this);
            
            var defaultStates = StateHelper.GetDefaultStatesFromDB(defaultStatesList);

            foreach (var state in defaultStates) {
                if (state is GroundStateSO so) {
                    GroundState = so;
                }
            }
            
            CurrentLocoStateDriver = GroundStateDriver;
            CurrentLocoStateDriver.EnterState();
        }

        public void SetContext(in LocoStateContext context) {
            Ctx = context;
        }

        
    }
}