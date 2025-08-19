using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "GroundState", menuName = "Spellbound/LocoStates/GroundState")]
    public class GroundStateSO : BaseLocoStateSO {
        public override void Initialize() {
            
        }
        
        public override void EnterStateLogic() {
            StateHelper.NotifyLocoStateChange(this);
        }
        
        public override void UpdateStateLogic(in LocoStateContext ctx) {
            
        }
        
        public override void FixedUpdateStateLogic(in LocoStateContext ctx) {
            Debug.Log($"Reading input: {ctx.MoveInput.x}");
        }
        
        public override void CheckSwitchStateLogic(in LocoStateContext ctx) {
            
        }
        
        public override void ExitStateLogic() {
            
        }
    }
}