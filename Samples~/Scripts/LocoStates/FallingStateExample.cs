using UnityEngine;

namespace SpellBound.Controller.Samples {
    /// <summary>
    /// This method is supposed to demonstrate how easy it is to make a new falling state that can alter the behavior
    /// of how this state interprets movement compared to the ground state examples.
    /// </summary>
    [CreateAssetMenu(fileName = "FallingStateExample", menuName = "Spellbound/StateMachine/FallingStateExample")]
    public class FallingStateExample : BaseLocomotionStateExample {
        protected override void EnterStateLogic() {
            // Here we show that it's as simple as changing a protected variable in this state to impact player movement
            // that will only apply in this state. We could override the handle input as well.
            HSpeedModifier = 0.3f;
        }

        protected override void UpdateStateLogic() {
            if (Ctx.StateData.Grounded)
                Ctx.locoStateMachine.ChangeState(LocoStateTypes.Grounded);
        }

        /// <summary>
        /// Physics update override.
        /// </summary>
        /// <remarks>
        /// We chose not to run the KeepCapsuleFloating(); here too because when the player is falling, we have no need
        /// to run calculations for a force correction.
        /// </remarks>
        protected override void FixedUpdateStateLogic() {
            PerformGroundCheck();
            HandleInput();
            HandleCharacterRotation();
        }

        protected override void ExitStateLogic() {
            // Then just change it back.
            HSpeedModifier = 1.0f;
        }
    }
}