// Copyright 2026 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// One action slot-to-state binding for the sample config.
    /// </summary>
    [Serializable]
    public sealed class ActionStateBindingExample : IStateBinding<ActionStateTypes> {
        [SerializeField] private ActionStateTypes slot;
        [SerializeField] private BaseSoState variant;
        public ActionStateTypes Slot => slot;
        public BaseSoState Variant => variant;
    }

    /// <summary>
    /// Sample reusable config for the action state machine.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionStateConfigExample", menuName = "Spellbound/StateMachine/Action Config Example")]
    public sealed class ActionStateConfigExample : ScriptableObject, IStateMachineConfig<ActionStateTypes> {
        [SerializeField] private List<ActionStateBindingExample> bindings = new();
        [SerializeField] private ActionStateTypes initialState = ActionStateTypes.Ready;
        public IReadOnlyList<IStateBinding<ActionStateTypes>> Bindings => bindings;
        public ActionStateTypes InitialState => initialState;
    }
}
