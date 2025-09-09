using System.Collections.Generic;
using Spellbound.Controller.PlayerController;
using SpellBound.Controller.PlayerController;
using SpellBound.Controller.PlayerInputs;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    public sealed class PlayerControllerExample : ControllerBase, IDebuggingInfo {
        [Header("Input Reference:")]
        [field: SerializeField] public PlayerInputActionsSO input { get; private set; }
        [Header("Camera Follow Reference:")]
        [field: SerializeField] public Transform referenceTransform { get; private set; }
        [Header("Rigidbody Reference:")]
        [field: SerializeField] public Rigidbody Rb { get; private set; }
        [Header("Collider Settings:")] 
        [field: SerializeField] public ResizableCapsuleCollider ResizableCapsuleCollider { get; private set; }
        [Header("Layer Settings:")] 
        [field: SerializeField] public LayerData LayerData { get; private set; }
        [Header("Rigidbody Settings:")] 
        [field: SerializeField] public RigidbodyData RigidbodyData { get; private set; }
        [Header("Character Rotation Settings:")] 
        [field: SerializeField] public RotationData RotationData { get; private set; }
        [Header("Stat Settings:")] 
        [field: SerializeField] public StatData StatData { get; private set; }
        [Header("State Settings:")] 
        [field: SerializeField] public StateData StateData { get; private set; }
        
        public StateMachine<LocoStateTypes> locoStateMachine { get; private set; }
        
        [Header("Locomotion States")]
        [SerializeField] private List<BaseSoState> locoStates;
        [SerializeField] private BaseSoState locoInitial;

        [Header("Action States")]
        /*[SerializeField] private List<BaseSoState> actionStates;
        [SerializeField] private BaseSoState actionInitial;*/
        
        [Header("Animator")]
        [SerializeField] private Animator animator;

        // What direction is up from the player?
        public Vector3 planarUp { get; private set; }
        
        private void Awake() {
            planarUp = transform.up;
            
            if (input == null)
                Debug.LogError("Please drag and drop an input reference in the CharacterController", this);
            
            Rb = GetComponent<Rigidbody>();
            Rb.freezeRotation = true;
            Rb.useGravity = true;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }

        public void FixedUpdate() {
            if (input.Direction == Vector3.zero)
                return;

            UseGroundModifiedVariant(false);
        }
        
        public void UseGroundModifiedVariant(bool reenterIfActive) {
            locoStateMachine?.SwapToVariantState(locoStates[1]);
        }
        
        protected override void ConfigureStateMachines(ControllerBase ctx, IList<IStateMachineRunner> machines) {
            locoStateMachine ??= StateMachine<LocoStateTypes>.CreateFromStates(
                    ctx, 
                    locoStates, 
                    LocoStateTypes.Grounded);
            
                machines.Add(locoStateMachine);

            /*var action = StateMachine.CreateFromStates(ctx, actionStates, actionInitial);
            if (action != null) 
                machines.Add(action);*/
        }

        public enum LocoStateTypes {
            Grounded,
            Falling
        }

        public void RegisterDebugInfo(SbPlayerDebugHudBase hud) {
            hud.Field("Controller.LocoState", () => {
                var n = locoStateMachine != null 
                        && locoStateMachine.CurrentDriver != null 
                        && locoStateMachine.CurrentDriver.State != null
                        ? locoStateMachine.CurrentDriver.State.AssetName
                        : "-";
                return n;
            });
        }
    }
}