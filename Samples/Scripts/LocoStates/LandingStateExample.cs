using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// I partitioned this state to be the in-between of the state before it and grounding in the event you wanted to
    /// play a ground sound or a ground animation or delay the player from being able to jump immediately.
    /// </summary>
    [CreateAssetMenu(fileName = "LandingStateExample", menuName = "Spellbound/StateMachine/LandingStateExample")]
    public class LandingStateExample : BaseSoState {
        protected new PlayerControllerExample Ctx;
        
        protected override void OnCtxInitialized() {
            Ctx = base.Ctx as PlayerControllerExample;
        }

        protected override void EnterStateLogic() {
            
        }
        protected override void UpdateStateLogic() {
            
        }
        protected override void FixedUpdateStateLogic() {
            
        }
        protected override void ExitStateLogic() {
            
        }
    }
}