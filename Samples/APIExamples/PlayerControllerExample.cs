// Copyright 2025 Spellbound Studio Inc.

using System;
using Spellbound.Core.Logging;
using Spellbound.Core.Tooling;
using Unity.Cinemachine;
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
        public ExampleInputManager ExampleInput { get; private set; }

        [Header("Camera:")]
        [field: SerializeField]
        public CameraData CameraData { get; private set; }

        [SerializeField] private Vector3 cameraOffset;
        private CameraController _camera;
        private CameraRigManager _cameraRig;
        private CinemachineBrain _brain;

        private Transform _tr;

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

        public StateMachine<PlayerControllerExample, LocoStateTypes> LocoStateMachine { get; private set; }
        public StateMachine<PlayerControllerExample, ActionStateTypes> ActionStateMachine { get; private set; }

        [Header("State Configs")]
        [SerializeField] private LocoStateConfigExample locoConfig;

        [SerializeField] private ActionStateConfigExample actionConfig;

        [Header("Animator"), SerializeField] private Animator animator;

        // What direction is up from the player?
        public Vector3 PlanarUp { get; private set; }

        /// <summary>
        /// The live camera's transform — the basis states use for camera-relative movement.
        /// </summary>
        public Transform ReferenceTransform => _cameraRig != null
                ? _cameraRig.CurrentCameraTransform
                : null;

        private void Awake() {
            _tr = transform;
            PlanarUp = _tr.up;

            if (ExampleInput == null) {
                if (!SingletonManager.TryGetSingletonInstance<ExampleInputManager>(out var im)) {
                    Log.Error("ExampleInput is missing in the scene most likely.");
                    return;
                }

                ExampleInput = im;
            }

            Rb = GetComponent<Rigidbody>();
            Rb.freezeRotation = true;
            Rb.useGravity = true;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;

            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }

        private void Start() {
            ConfigureStateMachines();
            InitCamera();
        }

        public void Update() {
            LocoStateMachine.UpdateStateMachine();
            ActionStateMachine.UpdateStateMachine();
        }

        public void FixedUpdate() {
            LocoStateMachine.FixedUpdateStateMachine();
            ActionStateMachine.FixedUpdateStateMachine();
        }
        
        private void LateUpdate() {
            if (_camera == null)
                return;

            _camera.TrackTarget();

            if (CameraData.FollowMouse)
                _camera.ApplyLook(ExampleInput.LookDirection);
        }

        private void OnDestroy() {
            LocoStateMachine?.Dispose();
            ActionStateMachine?.Dispose();
            _camera?.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (ResizableCapsuleCollider == null || !TryGetComponent(out CapsuleCollider _))
                return;

            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }
#endif

        /// <summary>
        /// Wires the standalone <see cref="CameraController"/> to the rig and starts driving it. The player owns no
        /// camera logic beyond feeding input — swap the rig or the controller to change camera behaviour entirely.
        /// </summary>
        private void InitCamera() {
            _cameraRig = CameraRigManager.Instance;

            if (_cameraRig == null) {
                Log.Error("CameraRigManager is missing from the scene.");
                return;
            }

            Cursor.lockState = CameraData.LockCursorOnStart
                    ? CursorLockMode.Locked
                    : CursorLockMode.None;

            _camera = new CameraController(_cameraRig, _tr, CameraData, cameraOffset);

            if (!_brain && Camera.main)
                Camera.main.TryGetComponent(out _brain);

            if (!_brain)
                _brain = FindAnyObjectByType<CinemachineBrain>();

            if (_brain && _camera.Pivot != null)
                _brain.WorldUpOverride = _camera.Pivot;
        }

        public void SetCameraFollowMouse(bool follow) => CameraData.FollowMouse = follow;

        private void ConfigureStateMachines() {
            LocoStateMachine = new StateMachine<PlayerControllerExample, LocoStateTypes>(this);
            LocoStateMachine.Configure(locoConfig);

            ActionStateMachine = new StateMachine<PlayerControllerExample, ActionStateTypes>(this);
            ActionStateMachine.Configure(actionConfig);
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
                var currentStateVariant = LocoStateMachine.GetCurrentRunningState();

                return currentStateVariant != null
                        ? currentStateVariant.name
                        : "None";
            });

            // Show each driver (enum value) and what variant it's pointing to
            foreach (LocoStateTypes stateType in Enum.GetValues(typeof(LocoStateTypes))) {
                debugHud.Field($"{stateType}", () => {
                    var currentVariant = LocoStateMachine.GetCurrentVariant(stateType);

                    return currentVariant != null
                            ? currentVariant.name
                            : "Not Assigned";
                });
            }

            // Repeat for action state machine.
            debugHud.Field("Current Action State", () => {
                var currentStateVariant = ActionStateMachine.GetCurrentRunningState();

                return currentStateVariant != null
                        ? currentStateVariant.name
                        : "None";
            });

            foreach (ActionStateTypes stateType in Enum.GetValues(typeof(ActionStateTypes))) {
                debugHud.Field($"{stateType}", () => {
                    var currentVariant = ActionStateMachine.GetCurrentVariant(stateType);

                    return currentVariant != null
                            ? currentVariant.name
                            : "Not Assigned";
                });
            }

            debugHud.Gizmo(() => {
                var capsule = ResizableCapsuleCollider;

                if (capsule.collider == null)
                    return;

                var floatData = capsule.CapsuleFloatData;
                var up = PlanarUp.sqrMagnitude > 0.5f
                        ? PlanarUp
                        : transform.up;
                var origin = capsule.collider.bounds.center;
                var radius = floatData.ProbeRadius;

                var maxReach = floatData.RideHeight *
                               ControllerHelper.GetSlopeReachFactor(StatData.maxSlopeAngle, StatData.maxSlopeAngle) +
                               floatData.GroundedTolerance + radius;
                var probe = capsule.ProbeGround(-up, maxReach, LayerData.GroundLayer);

                var angle = probe.HasHit
                        ? ControllerHelper.GetSlopeAngle(probe.Normal, up)
                        : 0f;
                var reach = floatData.RideHeight * ControllerHelper.GetSlopeReachFactor(angle, StatData.maxSlopeAngle) +
                            floatData.GroundedTolerance;

                Gizmos.color = probe.HasHit && probe.Distance <= reach
                        ? Color.green
                        : Color.red;
                Gizmos.DrawWireSphere(origin, radius);
                Gizmos.DrawLine(origin, origin - up * reach);
                Gizmos.DrawWireSphere(origin - up * (reach - radius), radius);

                if (!probe.HasHit)
                    return;

                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(origin - up * (probe.Distance - radius), radius);
                Gizmos.DrawLine(probe.Point, probe.Point + probe.Normal * 0.5f);
            });
        }
    }
}
