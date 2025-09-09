using System.Collections.Generic;
using SpellBound.Controller.PlayerController;

namespace SpellBound.Controller.PlayerStateMachine {
    /// <summary>
    /// Provider that supplies the machine with the initial state id and all available drivers.
    /// Implemented by the GAME layer (code or asset-backed).
    /// </summary>
    public interface IStateSetProvider {
        public string InitialStateId { get; }
        public IEnumerable<IStateDriver> GetDrivers(ControllerBase ctx);
    }
}