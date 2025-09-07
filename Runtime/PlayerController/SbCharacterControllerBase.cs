using System;
using System.Collections.Generic;
using SpellBound.Controller.ManagersAndStatics;
using Spellbound.Controller.PlayerController;
using SpellBound.Controller.PlayerInputs;
using SpellBound.Controller.PlayerStateMachine;
using UnityEngine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    /// <summary>
    /// Input and stats meet here to inform supporting members.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public abstract class SbCharacterControllerBase : MonoBehaviour, IDebuggingInfo {
        [Header("Input Reference:"), Tooltip("Please drag and drop.")]
        [field: SerializeField] public PlayerInputActionsSO input { get; private set; }
        
        [Header("Camera Follow Reference:")]
        [field: SerializeField] public Transform referenceTransform { get; private set; }
        
        [Header("Collider Settings:")] 
        [field: SerializeField] public ResizableCapsuleCollider ResizableCapsuleCollider { get; private set; }
        [field: SerializeField] public LayerData LayerData { get; private set; }
        
        [field: SerializeField] public RigidbodyData RigidbodyData { get; private set; }
        
        [field: SerializeField] public RotationData RotationData { get; private set; }
        
        [field: SerializeField] public StatData StatData { get; private set; }
        [field: SerializeField] public StateData StateData { get; private set; }
        
        public Rigidbody Rb { get; private set; }
        private AnimationControllerBase _animator;
        
        private LocoStateMachine _locoStateMachine;
        private ActionStateMachine _actionStateMachine;
        
        private BaseLocoStateSO _currentLocoState;
        private BaseActionStateSO _currentActionState;
        
        private readonly List<string> _defaultLocoStatesList = new() {
                StateHelper.DefaultGroundStateSO,
                StateHelper.DefaultFallingStateSO,
                StateHelper.DefaultJumpingStateSO,
                StateHelper.DefaultLandingStateSO,
                StateHelper.DefaultSlidingStateSO
        };

        private readonly List<string> _defaultActionStatesList = new() {
                StateHelper.DefaultReadyStateSO,
                StateHelper.DefaultGCDStateSO,
                StateHelper.DefaultInteractStateSO,
        };
        
        private Transform _tr;
        private Vector3 _horizontalVelocity;
        public Vector3 planarUp { get; private set; }
        
        /// <summary>
        /// Derived classes must provide the correct animation controller.
        /// </summary>
        protected abstract AnimationControllerBase CreateAnimationController();
        
        private void Awake() {
            _tr = transform;
            planarUp = _tr.up;
            
            if (input == null)
                Debug.LogError("Please drag and drop an input reference in the CharacterController", this);
            
            Rb = GetComponent<Rigidbody>();
            Rb.freezeRotation = true;
            Rb.useGravity = true;
            Rb.interpolation = RigidbodyInterpolation.Interpolate;
            
            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }
        
#if UNITY_EDITOR
        private void OnValidate() {
            _tr = transform;
            
            ResizableCapsuleCollider.Initialize(gameObject);
            ResizableCapsuleCollider.CalculateCapsuleColliderDimensions();
        }
#endif
        
        private void OnEnable() {
            StateHelper.OnLocoStateChange += HandleLocoStateChanged;
            StateHelper.OnActionStateChange += HandleActionStateChanged;
            
            if (!input) 
                return;

            input.OnJumpInput += HandleJumpPressed;
            input.OnInteractPressed += HandleInteractPressed;
            input.OnHotkeyOnePressed += HandleHotkeyOnePressed;
        }

        private void OnDisable() {
            StateHelper.OnLocoStateChange -= HandleLocoStateChanged;
            StateHelper.OnActionStateChange -= HandleActionStateChanged;
            
            _animator?.DisposeEvents();

            if (!input) 
                return;

            input.OnJumpInput -= HandleJumpPressed;
            input.OnInteractPressed -= HandleInteractPressed;
            input.OnHotkeyOnePressed -= HandleHotkeyOnePressed;
        }
        
        private void Start() {
            referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;

            _animator = CreateAnimationController();
            
            if (_animator == null)
                Debug.LogError("CharacterControllerBase requires an animator via the CreateAnimationController override.", 
                        this);
            
            _locoStateMachine = new LocoStateMachine(this, _defaultLocoStatesList);
            _actionStateMachine = new ActionStateMachine(this, _defaultActionStatesList);
        }
        
        private void Update() {
            _locoStateMachine.CurrentLocoStateDriver.UpdateState();
            _actionStateMachine.CurrentActionStateDriver.UpdateState();
        }
        
        private void FixedUpdate() {
            _locoStateMachine.CurrentLocoStateDriver.FixedUpdateState();
            _actionStateMachine.CurrentActionStateDriver.FixedUpdateState();
        }
        
        private void HandleLocoStateChanged(BaseLocoStateSO state) => _currentLocoState = state;
        private void HandleActionStateChanged(BaseActionStateSO state) => _currentActionState = state;
        
        

        private void HandleCharacterTurnTowardsHorizontalVelocity() {
            
        }
        
        
        
        public Transform GetReferenceTransform() => referenceTransform;
        
        public void SetSensorRange(Helper.RaycastLength sensorLength) {
            switch (sensorLength) {
                case Helper.RaycastLength.Normal:
                    
                    break;
                case Helper.RaycastLength.Extended:
                    
                    break;
                case Helper.RaycastLength.Retracted:
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sensorLength), sensorLength, null);
            }
        }

        #region StateEvaluaters
        private void HandleJumpPressed() {
            if (!StateData.Grounded)
                return;

            if (!ResourceCheck())
                return;
            
            //jumpFlag = true;
        }
        

        private bool ResourceCheck() {
            return true;
        }
        
        private void HandleInteractPressed() {
            /*if (interactFlagged)
                return;
            
            interactFlagged = true;*/
        }

        private void HandleHotkeyOnePressed() {
            /*if (hotkeyOneFlagged)
                return;
            
            if (!Physics.Raycast(
                        referenceTransform.position,
                        input.LookDirection,
                        out var hit,
                        10f,
                        1 << 6,
                        QueryTriggerInteraction.Ignore))
                return;

            hotkeyOneFlagged = true;*/
        }
        #endregion

        /// <summary>
        /// Runs if the debugger is attached.
        /// </summary>
        public void RegisterDebugInfo(SbPlayerDebugHudBase hud) {
            hud.Field("Controller.LocoState", () => {
                var lsName = _currentLocoState ? _currentLocoState.name : "-";
                return lsName;
            });
            
            hud.Field("Controller.ActionState", () => {
                var asName = _currentActionState ? _currentActionState.name : "-";
                return asName;
            });
            
            hud.Field("Controller.HorizontalSpeed", () => {
                if (Rb == null) 
                    return "-";
                
                var v = Rb.linearVelocity;
                var hMag = Vector3.ProjectOnPlane(v, planarUp).magnitude;
                return hMag.ToString("F2");
            });
            
            hud.Field("Controller.VerticalSpeed", () => {
                if (Rb == null) 
                    return "-";
                
                var v = Rb.linearVelocity;
                var vMag = Vector3.Dot(v, planarUp);
                return vMag.ToString("F2");
            });

            // Jump force (current configured value)
            hud.Field("Controller.JumpForce", () => StatData.jumpForce.ToString("F2"));
            
            hud.Gizmo(() => {
                var origin = ResizableCapsuleCollider.CapsuleColliderData.Collider.bounds.center;
                var dir = -planarUp;

                var rayLen = ResizableCapsuleCollider.SlopeData.RayDistance;
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, origin + dir * rayLen);
                Gizmos.DrawSphere(origin + dir * rayLen, 0.06f);
            });
        }
    }
}