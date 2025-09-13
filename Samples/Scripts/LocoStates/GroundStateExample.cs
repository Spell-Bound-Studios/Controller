using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "GroundStateExample", menuName = "Spellbound/StateMachine/GroundStateExample")]
    public class GroundStateExample : BaseLocoStateExample {
        protected override void EnterStateLogic() {
            Ctx.input.OnInteractPressed += HandleInteractPressed;
            Ctx.input.OnJumpInput += HandleJumpPressed;
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.StateData.Grounded)
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Falling);
        }

        protected override void ExitStateLogic() {
            Ctx.input.OnInteractPressed -= HandleInteractPressed;
            Ctx.input.OnJumpInput -= HandleJumpPressed;
        }

        protected virtual void HandleInteractPressed() {
            Ctx.locoStateMachine.ChangeVariant(LocoStateTypes.Grounded, Ctx.locoStates[1]);
        }
        
        private void HandleJumpPressed() {
            Ctx.locoStateMachine.ChangeState(LocoStateTypes.Jumping);
        }
    }
}