using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpellBound.Controller {
    /// <summary>
    /// This state machine is intended to be driven by a MonoBehaviour and offer simple public methods to change states
    /// and state variants. Additional information on the difference between "drivers", "states" and "variants" can be
    /// found in the read me.
    /// </summary>
    /// <typeparam name="TContext">
    /// TContext is expected to be a class of some kind that the user defines. We find that often times the controller
    /// is the context required to run the state machine... however, we wanted to give the user complete flexibility
    /// and allow them to containerize their own context should they choose.
    /// </typeparam>
    /// <typeparam name="TStateEnum">
    /// TStateEnum is a user-defined enum. The state machine will instantiate one BaseStateDriver class per item in the
    /// enum. We chose an enum because we find that it simplifies readability and allows the user to conceptualize
    /// what it is that they are trying to make.
    /// </typeparam>
    /// Usage:
    /// var myStateMachine = new StateMachine<MyContextClass, MyEnum>(myContext);
    /// myStateMachine.SetInitialVariant(MyEnum.Item, myBaseStateSo);
    /// myStateMachine.ChangeState(MyEnum.Item);
    public sealed class StateMachine<TContext, TStateEnum> where TContext : class where TStateEnum : Enum {
        public BaseStateDriver CurrentActiveDriver { get; private set; }
        public TContext ctx { get; private set; }
        
        private readonly Dictionary<TStateEnum, BaseStateDriver> _stateDrivers;
        private readonly Dictionary<Type, BaseSoState> _statesByType = new();

        public StateMachine(TContext ctx) {
            this.ctx = ctx;
            _stateDrivers = new Dictionary<TStateEnum, BaseStateDriver>();

            InitializeStateDrivers();
        }
        
        /// <summary>
        /// Automatically creates a state driver for each value in the enum.
        /// </summary>
        private void InitializeStateDrivers() {
            foreach (TStateEnum stateType in Enum.GetValues(typeof(TStateEnum)))
                _stateDrivers[stateType] = new BaseStateDriver();
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

        public void RegisterState(BaseSoState state) {
            if (state == null) {
                Debug.LogError($"The state: {state.AssetName} that you're trying to register is null.");
                return;
            }
            
            var stateType = state.GetType();
            
            if (!_statesByType.TryAdd(stateType, state)) {
                Debug.LogError($"The state dictionary already contains {stateType}.");
                return;
            }

            state.InitializeWithContext(ctx);
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
        /// Helper method to get a registered state.
        /// </summary>
        public BaseSoState GetRegisteredState<T>() where T : BaseSoState => _statesByType.GetValueOrDefault(typeof(T));
        
        /// <summary>
        /// 
        /// </summary>
        public void ChangeVariant(TStateEnum stateType, BaseSoState newVariant) {
            if (!_stateDrivers.TryGetValue(stateType, out var driver)) {
                Debug.LogError($"No state driver registered for state type: {stateType}");
                return;
            }
            
            if (newVariant.Ctx == null)
                newVariant.InitializeWithContext(ctx);
            
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

        public BaseSoState GetCurrentRunningState() => CurrentActiveDriver?.CurrentVariant;
        
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
        /// Registers a state driver for a specific state type if the user wants to create a custom state driver.
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