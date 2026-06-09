// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core.Logging;

namespace Spellbound.Controller {
    /// <summary>
    /// An enum-keyed state machine whose slots are each filled by a swappable state SO.
    /// </summary>
    /// <typeparam name="TContext">The object the states act on — usually the owning controller.</typeparam>
    /// <typeparam name="TStateEnum">The enum of state slots; one driver is created per value.</typeparam>
    public sealed class StateMachine<TContext, TStateEnum> where TContext : class where TStateEnum : Enum {
        /// <summary>
        /// The driver of the currently-active slot.
        /// </summary>
        public BaseStateDriver CurrentActiveDriver { get; private set; }

        /// <summary>
        /// The context passed to every state.
        /// </summary>
        public TContext ctx { get; private set; }

        /// <summary>
        /// The currently-active slot.
        /// </summary>
        public TStateEnum CurrentStateType { get; private set; }

        private readonly Dictionary<TStateEnum, BaseStateDriver> _stateDrivers;
        private readonly Dictionary<Type, BaseSoState> _statesByType = new();
        private readonly Dictionary<TStateEnum, BaseSoState> _defaultVariants = new();

        public StateMachine(TContext ctx) {
            this.ctx = ctx;
            _stateDrivers = new Dictionary<TStateEnum, BaseStateDriver>();

            InitializeStateDrivers();
        }

        #region Configuration

        /// <summary>
        /// Wires the machine from a config asset and enters its initial state.
        /// </summary>
        public void Configure(IStateMachineConfig<TStateEnum> config) {
            if (config == null) {
                Log.Error("Configure called with a null config; the state machine has no states.");

                return;
            }

            ConfigureCore(config.Bindings, config.InitialState);
        }

        /// <summary>
        /// Wires the machine from an inline binding list and enters the initial state.
        /// </summary>
        public void Configure<TBinding>(List<TBinding> bindings, TStateEnum initialState)
                where TBinding : class, IStateBinding<TStateEnum> =>
                ConfigureCore(bindings, initialState);

        /// <summary>
        /// Sets a slot's default variant and makes it the slot's current variant.
        /// </summary>
        public void SetInitialVariant(TStateEnum stateType, BaseSoState initialVariant) {
            if (!_stateDrivers.TryGetValue(stateType, out var driver)) {
                Log.Error($"No state driver found for state type: {stateType}");

                return;
            }

            if (initialVariant == null)
                return;

            _defaultVariants[stateType] = initialVariant;

            if (initialVariant.Ctx == null)
                initialVariant.InitializeWithContext(ctx);

            driver.ChangeVariant(initialVariant);
        }

        /// <summary>
        /// Registers a variant by its concrete type so it can be resolved later. Idempotent.
        /// </summary>
        public void RegisterState(BaseSoState state) {
            if (state == null) {
                Log.Error("Attempted to register a null state.");

                return;
            }

            var stateType = state.GetType();

            if (_statesByType.TryGetValue(stateType, out var existing)) {
                if (!ReferenceEquals(existing, state))
                    Log.Warn($"Two different assets of type {stateType.Name} were registered; keeping the first.");

                return;
            }

            _statesByType[stateType] = state;

            if (state.Ctx == null)
                state.InitializeWithContext(ctx);
        }

        /// <summary>
        /// Replaces the driver for a slot. Advanced use only.
        /// </summary>
        public void RegisterStateDriver(TStateEnum stateType, BaseStateDriver stateDriver) {
            if (_stateDrivers.ContainsKey(stateType))
                Log.Warn($"State driver for {stateType} is already registered. Overwriting.");

            _stateDrivers[stateType] = stateDriver;
        }

        #endregion

        #region Transitions & Variants

        /// <summary>
        /// Activates a different slot.
        /// </summary>
        public void ChangeState(TStateEnum newStateType) {
            if (!_stateDrivers.TryGetValue(newStateType, out var newDriver)) {
                Log.Error($"No state driver registered for state type: {newStateType}");

                return;
            }

            if (CurrentActiveDriver == newDriver)
                return;

            CurrentActiveDriver?.OnBecomeInactive();
            CurrentActiveDriver = newDriver;
            CurrentStateType = newStateType;
            CurrentActiveDriver.OnBecomeActive();
        }

        /// <summary>
        /// Swaps the variant filling a slot, taking effect immediately if the slot is active.
        /// </summary>
        public void ApplyVariant(TStateEnum slot, BaseSoState variant) {
            if (!_stateDrivers.TryGetValue(slot, out var driver)) {
                Log.Error($"No state driver registered for state type: {slot}");

                return;
            }

            if (variant == null) {
                Log.Error($"ApplyVariant called with a null variant for slot {slot}; ignoring.");

                return;
            }

            RegisterState(variant);
            driver.ChangeVariant(variant);
        }

        /// <summary>
        /// Swaps a slot to its registered variant of type <typeparamref name="TVariant"/>.
        /// </summary>
        public void ApplyVariant<TVariant>(TStateEnum slot) where TVariant : BaseSoState {
            var variant = GetState<TVariant>();

            if (variant == null) {
                Log.Error($"No registered variant of type {typeof(TVariant).Name}; list it in the config or " +
                          "apply it by reference first.");

                return;
            }

            ApplyVariant(slot, variant);
        }

        /// <summary>
        /// Restores a slot to the default variant recorded at configure time.
        /// </summary>
        public void RestoreDefault(TStateEnum slot) {
            if (_defaultVariants.TryGetValue(slot, out var def) && def != null) {
                ApplyVariant(slot, def);

                return;
            }

            Log.Error($"No default variant recorded for slot {slot}; was the machine configured?");
        }

        #endregion

        #region Queries

        /// <summary>
        /// Returns the registered variant of type <typeparamref name="TVariant"/>, or null.
        /// </summary>
        public TVariant GetState<TVariant>() where TVariant : BaseSoState =>
                _statesByType.GetValueOrDefault(typeof(TVariant)) as TVariant;

        /// <summary>
        /// Returns the variant currently filling a slot.
        /// </summary>
        public BaseSoState GetCurrentVariant(TStateEnum stateType) =>
                _stateDrivers.TryGetValue(stateType, out var driver)
                        ? driver.CurrentVariant
                        : null;

        /// <summary>
        /// Returns the active slot's current variant.
        /// </summary>
        public BaseSoState GetCurrentRunningState() => CurrentActiveDriver?.CurrentVariant;

        #endregion

        #region Tick

        /// <summary>
        /// Drives the active state's update; call from MonoBehaviour.Update.
        /// </summary>
        public void UpdateStateMachine() => CurrentActiveDriver?.DriveUpdate();

        /// <summary>
        /// Drives the active state's fixed update; call from MonoBehaviour.FixedUpdate.
        /// </summary>
        public void FixedUpdateStateMachine() => CurrentActiveDriver?.DriveFixedUpdate();

        #endregion

        #region Internal

        private void InitializeStateDrivers() {
            foreach (TStateEnum stateType in Enum.GetValues(typeof(TStateEnum)))
                _stateDrivers[stateType] = new BaseStateDriver();
        }

        private void ConfigureCore(IReadOnlyList<IStateBinding<TStateEnum>> bindings, TStateEnum initialState) {
            if (bindings == null) {
                Log.Error("Configure called with null bindings; the state machine has no states.");

                return;
            }

            var slotSeen = new HashSet<TStateEnum>();

            for (var i = 0; i < bindings.Count; i++) {
                var binding = bindings[i];
                var variant = binding?.Variant;

                if (variant == null) {
                    Log.Warn($"State binding at index {i} has a null variant; skipping.");

                    continue;
                }

                RegisterState(variant);

                if (slotSeen.Add(binding.Slot))
                    SetInitialVariant(binding.Slot, variant);
            }

            ChangeState(initialState);
        }

        #endregion
    }
}
