using System;
using SpellBound.Controller.PlayerController;

namespace SpellBound.Controller.PlayerStateMachine {
    public sealed class StateTypeDriver<StateTypes> : IVariantStateDriver where StateTypes : struct, Enum {
        public string Id { get; }
        public IState State { get; private set; }
        public StateTypes stateTypes { get; }

        private ControllerBase _ctx;

        public StateTypeDriver(StateTypes stateTypes, IState initialImpl) {
            this.stateTypes = stateTypes;
            State = initialImpl;
        }

        public void Enter(ControllerBase ctx) {
            _ctx = ctx; 
            State.OnEnter(_ctx);
        }

        public void Update() {
            State.OnUpdate();
        }

        public void FixedUpdate() {
            State.OnFixedUpdate();
        }

        public void Exit() {
            State.OnExit();
        }

        public void SetVariant(IState newState, ControllerBase ctx, bool reenterIfActive) {
            if (newState == null || ReferenceEquals(newState, State)) 
                return;

            if (reenterIfActive) {
                State.OnExit();
                State = newState;
                State.OnEnter(ctx);
            } 
            else {
                State = newState;
            }
        }
    }
}