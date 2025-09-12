using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// I partitioned this state to be the in-between of the ground state before it and falling or landing in the event
    /// you wanted to play a sound or play an animation.
    /// </summary>
    [CreateAssetMenu(fileName = "JumpStateExample", menuName = "Spellbound/StateMachine/JumpStateExample")]
    public class JumpStateExample : BaseSoState {
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