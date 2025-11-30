// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// This example is meant to be a semi-complete game ready drop in or rubric for any aspiring game dev to use as
    /// a reference or copy and paste. Our intention is to give you an idea of how to structure your MonoBehaviour to
    /// have it interact and direct a state machine based on your player's actions in-game. Please note that the getters
    /// and setters you see below are just preferences and inspired by Indie Wafflus. We make no claim to this being the
    /// best or a one-size-fit-all controller. However, we do think it will give you a good foundation to stand on and
    /// grow from whether you're looking to make a highly custom controller or just a controller that feels good.
    ///
    /// This example is also meant to show you how to create n-number of semi-orthogonal state machines that can
    /// compliment and react to one another. This really provides the user with a lot of control and allows the user to
    /// encapsulate states or expose them however they want to. For instance, I could make it so that the swimming state
    /// makes it so that the player can't do any combat or do a specific type of combat - the choice is yours!
    ///
    /// Please reference the documentation for additional details, or please feel free to use the discord and use this
    /// as a reference to see how others leverage these tools and solve game-specific challenges.
    /// </summary>
    public sealed class PlayerControllerExample : MonoBehaviour, IDebuggingInfo {
        [Header("Input Reference:")]
        [field: SerializeField]
        public InputManager input { get; private set; }

        [Header("Camera Follow Reference:")]
        [field: SerializeField]
        public Transform referenceTransform { get; private set; }

        [Header("Rigidbody Reference:")]
        [field: SerializeField]
        public Rigidbody Rb { get; private set; }

        [Header("Collider Settings:")]
        [field: SerializeField]
        public ResizableCapsuleCollider ResizableCapsuleCollider { get; private set; }

        [Header("Layer Settings:")]
        [field: SerializeField]
        public LayerData LayerData { get; private set; }

        [Header("Rigidbody Settings:")]
        [field: SerializeField]
        public RigidbodyData RigidbodyData { get; private set; }

        [Header("Character Rotation Settings:")]
        [field: SerializeField]
        public RotationData RotationData { get; private set; }

        [Header("Stat Settings:")]
        [field: SerializeField]
        public StatData StatData { get; private set; }

        [Header("State Settings:")]
        [field: SerializeField]
        public StateData StateData { get; private set; }

        public StateMachine<PlayerControllerExample, LocoStateTypes> locoStateMachine { get; private set; }
        public StateMachine<PlayerControllerExample, ActionStateTypes> actionStateMachine { get; private set; }

        [Header("Locomotion States")] public List<BaseSoState> locoStates;

        [Header("Action States")] public List<BaseSoState> actionStates;

        [Header("Animator"), SerializeField] private Animator animator;

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
            // Get your current camera from our built-in CameraRig or get your own custom camera and assign it here.
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;
            ConfigureStateMachines();
        }

        public void Update() {
            locoStateMachine.UpdateStateMachine();
            actionStateMachine.UpdateStateMachine();
        }

        public void FixedUpdate() {
            locoStateMachine.FixedUpdateStateMachine();
            actionStateMachine.FixedUpdateStateMachine();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }
#endif

        private void ConfigureStateMachines() {
            locoStateMachine = new StateMachine<PlayerControllerExample, LocoStateTypes>(this);
            locoStateMachine.SetInitialVariant(LocoStateTypes.Grounded, locoStates[0]);
            locoStateMachine.SetInitialVariant(LocoStateTypes.Falling, locoStates[2]);
            locoStateMachine.SetInitialVariant(LocoStateTypes.Jumping, locoStates[3]);
            locoStateMachine.SetInitialVariant(LocoStateTypes.Landing, locoStates[4]);
            // Initialize a state by calling the ChangeState method to get the machine going.
            locoStateMachine.ChangeState(LocoStateTypes.Falling);

            actionStateMachine = new StateMachine<PlayerControllerExample, ActionStateTypes>(this);
            actionStateMachine.SetInitialVariant(ActionStateTypes.Ready, actionStates[0]);
            actionStateMachine.ChangeState(ActionStateTypes.Ready);
        }

        /// <summary>
        /// Optional.
        /// This method comes from the IDebuggingInfo interface that we implemented. It allows the user to add or remove
        /// the debugging component at runtime should they choose to. It will simply allow you to print things to canvas
        /// in an easy and convenient way.
        /// </summary>
        public void RegisterDebugInfo(ControllerDebugging debugHud) {
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

            debugHud.Gizmo(() => {
                var origin = ResizableCapsuleCollider.collider.bounds.center;
                var dir = -planarUp;

                var rayLen = ResizableCapsuleCollider.SlopeData.RayDistance;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, origin + dir * rayLen);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(origin, 0.06f);
                Gizmos.DrawSphere(origin + dir * rayLen, 0.06f);
            });
        }
    }

    /// <summary>
    /// These are the enums belonging to our state machine example. You can put these anywhere - I chose to put them
    /// here as easy reference but feel free to put them anywhere in your game as long as they are accessible by the
    /// state machine and states you create.
    /// </summary>
    public enum LocoStateTypes {
        Grounded,
        Jumping,
        Falling,
        Landing
    }

    public enum ActionStateTypes {
        Ready
    }
}