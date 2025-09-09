using System;
using System.Collections.Generic;
using SpellBound.Controller.PlayerController;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class StateMachine<StateTypes> : IStateMachineRunner where StateTypes : struct, Enum {
        // The controller that provides context.
        public ControllerBase Ctx { get; } 
        
        // Drivers represent
        public IStateDriver CurrentDriver { get; private set; }
        
        private readonly Dictionary<StateTypes, IStateDriver> _stateTypesAndDrivers = new();

        public StateMachine(ControllerBase ctx, IEnumerable<IStateDriver> drivers, StateTypes initialRole) {
            Ctx = ctx; 

            foreach (var d in drivers) {
                if (d is not StateTypeDriver<StateTypes> typed) 
                    continue;
                
                _stateTypesAndDrivers.TryAdd(typed.stateTypes, d);
            }

            if (!_stateTypesAndDrivers.TryGetValue(initialRole, out var initial)) {
                Debug.LogError($"Initial role {initialRole} not found");
                return;
            }
            
            CurrentDriver = initial; 
            CurrentDriver.Enter(ctx);
        }

        public static StateMachine<StateTypes> CreateFromStates(ControllerBase ctx, IList<BaseSoState> states, StateTypes initialState) {
            if (ctx == null || states == null) 
                return null;
            
            // Create a new list of state drivers and a dictionary of drivers responsible for our states.
            // Remember, the state drivers are actually the backbone that drives the specific custom scriptable objects
            // that you create and allow you unlimited flexibility by simply defining new state-scriptable objects
            // while the drivers remain constant. Think of them as the engine.
            var roleToDriver = new Dictionary<StateTypes, IStateDriver>();
            var drivers = new List<IStateDriver>();
            
            foreach (var s in states) {
                if (s == null) 
                    continue;

                var typed = s as IStateType<StateTypes>;
                if (typed == null) {
                    Debug.LogError($"State {s.name} does not implement IStateRole<{typeof(StateTypes).Name}>");
                    continue;
                }

                if (roleToDriver.ContainsKey(typed.StateType)) 
                    continue;

                var rd = new StateTypeDriver<StateTypes>(typed.StateType, s);
                roleToDriver.Add(typed.StateType, rd);
                drivers.Add(rd);
            }

            if (drivers.Count != 0) 
                return new StateMachine<StateTypes>(ctx, drivers, initialState);

            Debug.LogError("StateMachine.CreateFromStates: no valid states provided.");
            return null;
        }
        
        public void Update() => CurrentDriver?.Update(); 
        public void FixedUpdate() => CurrentDriver?.FixedUpdate();

        public bool ChangeState(StateTypes nextState) {
            if (!_stateTypesAndDrivers.TryGetValue(nextState, out var next)) 
                return false;
            
            if (ReferenceEquals(next, CurrentDriver)) 
                return true;

            CurrentDriver?.Exit();
            CurrentDriver = next;
            CurrentDriver.Enter(Ctx);
            return true;
        }

        public bool SwapToVariantState(IState newState) {
            if (newState is not IStateType<StateTypes> typed)
                return false;

            if (!_stateTypesAndDrivers.TryGetValue(typed.StateType, out var driver))
                return false;

            if (driver is not IVariantStateDriver variantDriver)
                return false;

            var isActive = ReferenceEquals(driver, CurrentDriver);
            
            if (isActive) {
                CurrentDriver.Exit();
                variantDriver.SetVariant(newState, Ctx, false);
                CurrentDriver.Enter(Ctx);
            } 
            else {
                variantDriver.SetVariant(newState, Ctx, false);
            }

            return true;
        }
    }
}