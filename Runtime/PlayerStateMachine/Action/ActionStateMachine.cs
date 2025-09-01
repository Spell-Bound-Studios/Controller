using System.Collections.Generic;
using SpellBound.Controller.PlayerController;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class ActionStateMachine {
        public SbCharacterControllerBase CharController;
        
        public BaseActionStateDriver CurrentActionStateDriver;
        
        public ReadyStateDriver ReadyStateDriver;
        public ReadyStateSO ReadyState;
        
        public GCDStateDriver GCDStateDriver;
        public GCDStateSO GCDState;
        
        public InteractStateDriver InteractStateDriver;
        public InteractStateSO InteractState;
        
        public ActionStateMachine(SbCharacterControllerBase cc, List<string> defaultStatesList) {
            CharController = cc;
            
            ReadyStateDriver = new ReadyStateDriver(this);
            GCDStateDriver = new GCDStateDriver(this);
            InteractStateDriver = new InteractStateDriver(this);
            
            var defaultStates = StateHelper.GetDefaultActionStatesFromDB(defaultStatesList);

            foreach (var state in defaultStates) {
                switch (state) {
                    case ReadyStateSO rso:
                        ReadyState = rso;
                        break;
                    case GCDStateSO gso:
                        GCDState = gso;
                        break;
                    case InteractStateSO iso:
                        InteractState = iso;
                        break;
                }
            }
            
            if (defaultStates.Count <= 0)
                Debug.LogError($"Default state count found in constructor: {defaultStates.Count}. Please verify that" +
                                "default states list is created/exists by instantiator.");

            if (CurrentActionStateDriver != null) 
                return;

            CurrentActionStateDriver = ReadyStateDriver;
            CurrentActionStateDriver.EnterState();
        }

        public void ChangeState(BaseActionStateDriver newDriver) {
            CurrentActionStateDriver.ExitState();
            CurrentActionStateDriver = newDriver;
            CurrentActionStateDriver.EnterState();
        }
    }
}