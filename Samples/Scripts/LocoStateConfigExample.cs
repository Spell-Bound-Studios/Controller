// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// One locomotion slot-to-state binding for the sample config.
    /// </summary>
    [Serializable]
    public sealed class LocoStateBindingExample : IStateBinding<LocoStateTypes> {
        [SerializeField] private LocoStateTypes slot;
        [SerializeField] private BaseSoState variant;
        public LocoStateTypes Slot => slot;
        public BaseSoState Variant => variant;
    }

    /// <summary>
    /// Sample reusable config for the locomotion state machine.
    /// </summary>
    [CreateAssetMenu(fileName = "LocoStateConfigExample", menuName = "Spellbound/StateMachine/Loco Config Example")]
    public sealed class LocoStateConfigExample : ScriptableObject, IStateMachineConfig<LocoStateTypes> {
        [SerializeField] private List<LocoStateBindingExample> bindings = new();
        [SerializeField] private LocoStateTypes initialState = LocoStateTypes.Falling;
        public IReadOnlyList<IStateBinding<LocoStateTypes>> Bindings => bindings;
        public LocoStateTypes InitialState => initialState;
    }
}
