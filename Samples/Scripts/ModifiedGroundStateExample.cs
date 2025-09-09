using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "ModifiedGroundStateExample", menuName = "Spellbound/StateMachine/ModifiedGroundStateExample")]
    public sealed class ModifiedGroundStateExample : BaseSoState, IStateType<PlayerControllerExample.LocoStateTypes> {
        [SerializeField] private PlayerControllerExample.LocoStateTypes stateType;
        public PlayerControllerExample.LocoStateTypes StateType => stateType;
        
        protected override void EnterStateLogic() { }
        protected override void UpdateStateLogic() { }
        protected override void FixedUpdateStateLogic() { }
        protected override void ExitStateLogic() { }
    }
}