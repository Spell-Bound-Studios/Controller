using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public class FallingStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        
        protected override void OnCtxInitialized() {
            Ctx = base.Ctx as PlayerControllerExample;
        }
        
        protected override void EnterStateLogic() {
            
        }

        protected override void UpdateStateLogic() {
            if (Ctx.StateData.Grounded)
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Grounded);
        }

        protected override void FixedUpdateStateLogic() {
            
        }

        protected override void ExitStateLogic() {
            
        }
    }
}