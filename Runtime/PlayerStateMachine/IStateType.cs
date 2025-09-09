namespace SpellBound.Controller.PlayerStateMachine {
    public interface IStateType<out StateTypes> {
        StateTypes StateType { get; }
    }
}