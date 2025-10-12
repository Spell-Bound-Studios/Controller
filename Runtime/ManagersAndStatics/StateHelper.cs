// Copyright 2025 Spellbound Studio Inc.

namespace SpellBound.Controller {
    public static class StateHelper {
        /*public static event Action<BaseSoState> OnLocoStateChange = delegate { };
        public static event Action<BaseSoState> OnActionStateChange = delegate { };
        public static event Action<States> OnStateChanged = delegate { };
        public static event Action<float> OnAnimationSpeedChanged = delegate { };

        public const string DefaultGroundStateSO = "GroundState";
        public const string DefaultFallingStateSO = "FallingState";
        public const string DefaultLandingStateSO = "LandingState";
        public const string DefaultJumpingStateSO = "JumpingState";
        public const string DefaultSlidingStateSO = "SlidingState";

        public const string DefaultReadyStateSO = "ReadyState";
        public const string DefaultGCDStateSO = "GCDState";
        public const string DefaultInteractStateSO = "InteractState";

        public static readonly Dictionary<States, int> AnimationStateDict;

        public const States DefaultAnimationState = States.Grounded;

        [Serializable]
        public enum States {
            Grounded,
            Jumping,
            Falling,
            Landing,
            Casting,
            Attacking
        }

        static StateHelper() {
            AnimationStateDict = new Dictionary<States, int> {
                    { States.Grounded, Animator.StringToHash("grounded") },
                    { States.Jumping, Animator.StringToHash("jumping") },
                    { States.Falling, Animator.StringToHash("falling") },
                    { States.Landing, Animator.StringToHash("landing") },
                    { States.Casting, Animator.StringToHash("casting") },
                    { States.Attacking, Animator.StringToHash("attacking") },
            };
        }

        public static List<BaseLocoStateSO> GetDefaultLocoStatesFromDB(List<string> stateName) {
            var states = new List<BaseLocoStateSO>();

            foreach (var s in stateName) {
                if (!StateDatabase.Instance.TryGetLocoState(s, out var preset)) {
                    Debug.LogError($"No loco state with name {s}.");
                    return null;
                }

                states.Add(preset);
            }
            return states;
        }

        public static List<BaseActionStateSO> GetDefaultActionStatesFromDB(List<string> stateName) {
            var states = new List<BaseActionStateSO>();

            foreach (var s in stateName) {
                if (!StateDatabase.Instance.TryGetActionState(s, out var preset)) {
                    Debug.LogError($"No action state with name {s}.");
                    return null;
                }

                states.Add(preset);
            }
            return states;
        }

        public static void NotifyLocoStateChange(BaseLocoStateSO state) => OnLocoStateChange.Invoke(state);
        public static void NotifyLocoAnimationChange(States state) => OnStateChanged.Invoke(state);
        public static void NotifyLocoAnimationSpeedChange(float speed) => OnAnimationSpeedChanged.Invoke(speed);

        public static void NotifyActionStateChange(BaseActionStateSO state) => OnActionStateChange.Invoke(state);
        public static void NotifyActionAnimationChange(States state) => OnStateChanged.Invoke(state);
        public static void NotifyActionAnimationSpeedChange(float speed) => OnAnimationSpeedChanged.Invoke(speed);*/
    }
}