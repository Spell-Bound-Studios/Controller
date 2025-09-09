using SpellBound.Controller.PlayerController;

namespace SpellBound.Controller.PlayerStateMachine {
    public interface IVariantStateDriver : IStateDriver {
        public void SetVariant(IState newState, ControllerBase ctx, bool reenterIfActive);
    }
}