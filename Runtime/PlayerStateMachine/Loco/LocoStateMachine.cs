using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class LocoStateMachine {
        internal LocoStateContext Ctx;
        
        public BaseLocoStateDriver CurrentLocoStateDriver;
        
        public GroundStateDriver GroundStateDriver;
        public GroundStateSO GroundState;
        
        public FallingStateDriver FallingStateDriver;
        public FallingStateSO FallingState;
        
        public LocoStateMachine(List<string> defaultStatesList) {
            GroundStateDriver = new GroundStateDriver(this);
            FallingStateDriver = new FallingStateDriver(this);
            
            var defaultStates = StateHelper.GetDefaultStatesFromDB(defaultStatesList);

            foreach (var state in defaultStates) {
                switch (state) {
                    case GroundStateSO gso:
                        GroundState = gso;
                        break;
                    case FallingStateSO fso:
                        FallingState = fso;
                        break;
                }
            }
            
            if (defaultStates.Count <= 0)
                Debug.LogError($"Default state count found in constructor: {defaultStates.Count}. Please verify that" +
                                "default states list is created/exists by instantiator.");
            
            Debug.Log("DefaultStates found: " + defaultStatesList.Count);
            
            CurrentLocoStateDriver = FallingStateDriver;
            CurrentLocoStateDriver.EnterState();
        }

        public void SetContext(in LocoStateContext context) {
            Ctx = context;
        }

        
    }
}