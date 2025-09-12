using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class StateMachine<TContext, TStateEnum> where TContext : class where TStateEnum : Enum {
        public BaseStateDriver CurrentActiveDriver { get; private set; }
        public TContext ctx { get; }
        
        private readonly Dictionary<TStateEnum, BaseStateDriver> _stateDrivers;

        public StateMachine(TContext ctx) {
            this.ctx = ctx;
            _stateDrivers = new Dictionary<TStateEnum, BaseStateDriver>();

            InitializeStateDrivers();
        }
        
        /// <summary>
        /// Automatically creates a state driver for each value in the enum.
        /// </summary>
        private void InitializeStateDrivers() {
            foreach (TStateEnum stateType in Enum.GetValues(typeof(TStateEnum))) {
                _stateDrivers[stateType] = new BaseStateDriver();
            }
        }
        
        /// <summary>
        /// Sets the initial state variant for a specific state type.
        /// Call this during setup to assign ScriptableObject states to each state type.
        /// </summary>
        public void SetInitialVariant(TStateEnum stateType, BaseSoState initialVariant) {
            if (!_stateDrivers.TryGetValue(stateType, out var driver)) {
                Debug.LogError($"No state driver found for state type: {stateType}");
                return;
            }

            if (initialVariant == null) 
                return;

            initialVariant.InitializeWithContext(ctx);
            driver.ChangeVariant(initialVariant);
        }
        
        /// <summary>
        /// Changes to a different state type (different driver).
        /// </summary>
        public void ChangeState(TStateEnum newStateType) {
            if (!_stateDrivers.TryGetValue(newStateType, out var newDriver)) {
                Debug.LogError($"No state driver registered for state type: {newStateType}");
                return;
            }
            
            if (CurrentActiveDriver == newDriver) 
                return;
            
            CurrentActiveDriver?.OnBecomeInactive();
            CurrentActiveDriver = newDriver;
            CurrentActiveDriver.OnBecomeActive();
        }
        
        /// <summary>
        /// Changes the variant of a specific state type without changing the active state.
        /// </summary>
        public void ChangeVariant(TStateEnum stateType, BaseSoState newVariant) {
            if (!_stateDrivers.TryGetValue(stateType, out var driver)) {
                Debug.LogError($"No state driver registered for state type: {stateType}");
                return;
            }
            
            if (newVariant.Ctx == null) {
                newVariant.InitializeWithContext(ctx);
            }
            
            driver.ChangeVariant(newVariant);
        }
        
        /// <summary>
        /// Gets the current variant for a specific state type.
        /// </summary>
        public BaseSoState GetCurrentVariant(TStateEnum stateType) {
            return _stateDrivers.TryGetValue(stateType, out var driver) 
                ? driver.CurrentVariant 
                : null;
        }

        public BaseSoState GetCurrentRunningState() {
            return CurrentActiveDriver?.CurrentVariant;
        }
        
        /// <summary>
        /// Initializes all registered state variants with the context.
        /// Call this after registering all drivers and setting initial variants.
        /// </summary>
        public void InitializeAllStates() {
            foreach (var driverPair in _stateDrivers) {
                if (driverPair.Value.CurrentVariant != null) {
                    driverPair.Value.CurrentVariant.InitializeWithContext(ctx);
                }
            }
        }
        
        /// <summary>
        /// Call this from your MonoBehaviour's Update method.
        /// </summary>
        public void UpdateStateMachine() {
            CurrentActiveDriver?.DriveUpdate();
        }
        
        /// <summary>
        /// Call this from your MonoBehaviour's FixedUpdate method.
        /// </summary>
        public void FixedUpdateStateMachine() {
            CurrentActiveDriver?.DriveFixedUpdate();
        }
        
        /// <summary>
        /// Registers a state driver for a specific state type if the user wishes to create a custom state driver.
        /// This will likely go unused except for advanced users.
        /// </summary>
        public void RegisterStateDriver(TStateEnum stateType, BaseStateDriver stateDriver) {
            if (_stateDrivers.ContainsKey(stateType)) {
                Debug.LogWarning($"State driver for {stateType} is already registered. Overwriting.");
            }
            
            _stateDrivers[stateType] = stateDriver;
        }
    }
}