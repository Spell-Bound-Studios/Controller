using System;
using System.Collections.Generic;
using Spellbound.Controller.PlayerController;
using SpellBound.Controller.PlayerController;
using SpellBound.Controller.PlayerInputs;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    public sealed class PlayerControllerExample : MonoBehaviour, IDebuggingInfo {
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
        
        public StateMachine<PlayerControllerExample, LocoStateTypes> locoStateMachine { get; private set; }
        public StateMachine<PlayerControllerExample, ActionStateTypes> actionStateMachine { get; private set; }
        
        [Header("Locomotion States")]
        [SerializeField] private List<BaseSoState> locoStates;

        [SerializeField] private LocoStateTypes initialLocoState = LocoStateTypes.Grounded;

        [Header("Action States")]
        [SerializeField] private List<BaseSoState> actionStates;
        [SerializeField] private ActionStateTypes initialActionState = ActionStateTypes.Ready;
        
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

        private void Start() {
            ConfigureStateMachines();
        }

        public void Update() {
            locoStateMachine.UpdateStateMachine();
        }

        public void FixedUpdate() {
            locoStateMachine.FixedUpdateStateMachine();
            
            if (input.Direction == Vector3.zero)
                return;

            UseGroundModifiedVariant();
        }
        
        public void UseGroundModifiedVariant() {
            locoStateMachine?.ChangeVariant(LocoStateTypes.Grounded, locoStates[1]);
        }
        
        private void ConfigureStateMachines() {
            locoStateMachine = new StateMachine<PlayerControllerExample, LocoStateTypes>(this);
            locoStateMachine.SetInitialVariant(LocoStateTypes.Grounded, locoStates[0]);
            // Initialize a state by calling the ChangeState method to get the machine going.
            locoStateMachine.ChangeState(initialLocoState);

            actionStateMachine = new StateMachine<PlayerControllerExample, ActionStateTypes>(this);
            actionStateMachine.SetInitialVariant(ActionStateTypes.Ready, actionStates[0]);
            actionStateMachine.ChangeState(initialActionState);
        }

        public enum LocoStateTypes {
            Grounded,
            Falling,
            Swimming
        }

        public enum ActionStateTypes {
            Ready
        }

        public void RegisterDebugInfo(SbPlayerDebugHudBase debugHud) {
            // Show which ScriptableObject state is currently running
            debugHud.Field("Current Loco State", () => {
                var currentStateVariant = locoStateMachine.GetCurrentRunningState();
                return currentStateVariant != null 
                        ? currentStateVariant.AssetName 
                        : "None";
            });
            
            // Show each driver (enum value) and what variant it's pointing to
            foreach (LocoStateTypes stateType in Enum.GetValues(typeof(LocoStateTypes))) {
                debugHud.Field($"{stateType}", () => {
                    var currentVariant = locoStateMachine.GetCurrentVariant(stateType);
                    return currentVariant != null 
                            ? currentVariant.name 
                            : "Not Assigned";
                });
            }
            
            // Repeat for action state machine.
            debugHud.Field("Current Action State", () => {
                var currentStateVariant = actionStateMachine.GetCurrentRunningState();
                return currentStateVariant != null 
                        ? currentStateVariant.AssetName 
                        : "None";
            });
            
            foreach (ActionStateTypes stateType in Enum.GetValues(typeof(ActionStateTypes))) {
                debugHud.Field($"{stateType}", () => {
                    var currentVariant = actionStateMachine.GetCurrentVariant(stateType);
                    return currentVariant != null 
                            ? currentVariant.name 
                            : "Not Assigned";
                });
            }
        }
    }
}