using System.Collections.Generic;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Common controller base that all controllers should inherit from.
    /// </summary>
    public abstract class ControllerBase : MonoBehaviour {
        private readonly List<IStateMachineRunner> _machines = new();

        private void Start() {
            ConfigureStateMachines(this, _machines);
        }
        
        private void Update() {
            foreach (var machine in _machines) {
                machine.Update();
            }
        }

        private void FixedUpdate() {
            foreach (var machine in _machines) {
                machine.FixedUpdate();
            }
        }
        
        protected abstract void ConfigureStateMachines(ControllerBase ctx, IList<IStateMachineRunner> machines);
    }
}