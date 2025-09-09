using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public sealed class GroundStateExample : BaseSoState, IStateType<PlayerControllerExample.LocoStateTypes> {
        [SerializeField] private PlayerControllerExample.LocoStateTypes stateType = PlayerControllerExample.LocoStateTypes.Grounded;
        public PlayerControllerExample.LocoStateTypes StateType => stateType;
        protected override void EnterStateLogic() { }
        protected override void UpdateStateLogic() { }
        protected override void FixedUpdateStateLogic() { }
        protected override void ExitStateLogic() { }
    }
}