// Copyright 2025 Spellbound Studio Inc.

using System;
using System.Collections.Generic;
using Spellbound.Core;
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

        [Header("Camera References:")]
        [field: SerializeField] public Transform referenceTransform { get; private set; }
        [field: SerializeField] private Transform cameraPivot;
        [field: SerializeField] public CameraData CameraData { get; private set; }
        private CameraRigManager _cameraRig;
        private CinemachineBrain _brain;

        private Transform _tr;

        // Cached local rotation value X.
        private float _currentXAngle;

        // Cached local rotation value Y.
        private float _currentYAngle;

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

        [Header("Locomotion States")] 
        public List<BaseSoState> locoStates;

        [Header("Action States")] 
        public List<BaseSoState> actionStates;

        [Header("Animator"), SerializeField] 
        private Animator animator;

        // What direction is up from the player?
        public Vector3 planarUp { get; private set; }

        private void Awake() {
            _tr = transform;
            planarUp = _tr.up;

            if (ExampleInput == null) {
                if (!SingletonManager.TryGetSingletonInstance<ExampleInputManager>(out var im)) {
                    Debug.LogError("ExampleInput is missing in the scene most likely.", this);
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
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;
            ConfigureStateMachines();
            
            // Get your current camera from our built-in CameraRig or get your own custom camera and assign it here.
            CameraInit();
        }

        public void Update() {
            RotateCamera(ExampleInput.LookDirection.x, -ExampleInput.LookDirection.y);
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

        private void CameraInit()
        {
            Cursor.lockState = CameraData.cursorLockOnStart
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            if (ExampleInput == null)
                Debug.LogError("Please drag and drop an input reference in the CharacterController", this);

            if (!_brain && Camera.main)
                Camera.main.TryGetComponent(out _brain);

            if (!_brain)
                _brain = FindFirstObjectByType<CinemachineBrain>();

            if (!_brain)
                Debug.LogError("No brain found. CinemachineBrain missing from scene.", this);

            _currentXAngle = _tr.localRotation.eulerAngles.x;
            _currentYAngle = _tr.localRotation.eulerAngles.y;

            if (CameraRigManager.Instance == null) {
                Debug.LogError("CameraController has a dependency on CameraRigManager. Please ensure the camera rig" +
                               "prefab is in the scene or the CameraRigManager script is on your custom camera rig.",
                    this);
            }

            if (SyncTransform.Instance == null) {
                Debug.LogError("CameraController has a dependency on SyncTransform. Please ensure the CameraFollow" +
                               "prefab is in the scene or the SyncTransform script is on your custom object.",
                    this);
            }

            _cameraRig = CameraRigManager.Instance;

            if (ExampleInput)
                ExampleInput.OnMouseWheelInput += ZoomCamera;

            CameraSetup();
        }
        
        /// <summary>
        /// Rotates the camera based on the device horizontal and vertical input about the pivot.
        /// </summary>
        private void RotateCamera(float horizontalInput, float verticalInput) {
            if (!CameraData.cameraFollowMouse)
                return;

            var targetX = _currentXAngle + verticalInput * CameraData.cameraSpeed;
            var targetY = _currentYAngle + horizontalInput * CameraData.cameraSpeed;

            targetX = Mathf.Clamp(targetX, -CameraData.upperVerticalLimit, CameraData.lowerVerticalLimit);

            if (CameraData.smoothCameraRotation) {
                var blend = 1f - Mathf.Exp(-CameraData.cameraSmoothingFactor * Time.unscaledDeltaTime);
                _currentXAngle = Mathf.LerpAngle(_currentXAngle, targetX, blend);
                _currentYAngle = Mathf.LerpAngle(_currentYAngle, targetY, blend);
            }
            else {
                _currentXAngle = targetX;
                _currentYAngle = targetY;
            }

            _currentXAngle = Mathf.Clamp(_currentXAngle, -CameraData.upperVerticalLimit, CameraData.lowerVerticalLimit);
            cameraPivot.localRotation = Quaternion.Euler(_currentXAngle, _currentYAngle, 0f);
        }
        
        private void ZoomCamera(Vector2 zoomInput) {
            if (!CameraData.cameraFollowMouse)
                return;

            var currentZoom = _cameraRig.GetCurrentCameraZoom();

            if (float.IsNaN(currentZoom))
                return;

            var target = currentZoom - zoomInput.y * CameraData.zoomIncrement;
            target = Mathf.Clamp(target, CameraData.minZoomDistance, CameraData.maxZoomDistance);
            CameraRigManager.Instance.SetCameraZoom(target);
        }

        /// <summary>
        /// Sets our cameraRig tracking target.
        /// </summary>
        private void CameraSetup() {
            SyncTransform.Instance.SetFollowTransform(gameObject.transform);

            if (!cameraPivot)
                cameraPivot = SyncTransform.Instance.transform;

            _brain.WorldUpOverride = cameraPivot;

            if (_cameraRig == null) {
                Debug.LogError("Camera rig is null and doesn't appear to be in scene.");

                return;
            }

            _cameraRig.DefaultTarget.Target.TrackingTarget = cameraPivot;
        }

        public void SetCameraFollowMouse(bool follow) => CameraData.cameraFollowMouse = follow;
        public void SetCameraSpeed(float speed) => CameraData.cameraSpeed = speed;
        public float GetCameraSpeed() => CameraData.cameraSpeed;
        public void SetCameraSmooth(bool isSmooth) => CameraData.smoothCameraRotation = isSmooth;
        
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