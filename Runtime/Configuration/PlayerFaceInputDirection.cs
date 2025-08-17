using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    /// <summary>
    /// Intended to face the player in the direction of input. Rotations should only happen about the y axis.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerFaceInputDirection : MonoBehaviour {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Transform referenceTransform;
        [SerializeField] private float turnSpeed = 500f;

        private Transform _tr;
        private Vector3 _planarUp;
        private float _currentYRotation;
        private const float FallOffAngle = 90f;

        private void Awake() {
            _tr = transform;
            _planarUp = _tr.up;
        }
        
        private void Start() {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (referenceTransform == null) {
                referenceTransform = CameraRigManager.Instance.GetCurrentCamera().transform;
            }
            
            _currentYRotation = _tr.eulerAngles.y;
        }

        private void LateUpdate() {
            // Basically gives the x,z components of our velocity vector since they are normal to the up direction.
            var velocity = Vector3.ProjectOnPlane(
                    playerController.GetMovementVelocity(), _planarUp);
            
            // Return early if we're not moving.
            if (velocity.magnitude < 0.001f)
                return;
            
            var camForwardPlanar = Vector3.ProjectOnPlane(
                    referenceTransform.forward, _planarUp).normalized;

            if (camForwardPlanar.magnitude < 0.001f)
                camForwardPlanar = Vector3.ProjectOnPlane(
                        _tr.forward, _planarUp).normalized;
            
            var desiredFacingDir = velocity.normalized;
            
            var angleDiff = Helper.GetAngle(_tr.forward, desiredFacingDir, _planarUp);

            var step = Mathf.Sign(angleDiff) *
                       Mathf.InverseLerp(0f, FallOffAngle, Mathf.Abs(angleDiff)) *
                       Time.deltaTime * turnSpeed;
            
            _currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDiff) ? angleDiff : step;
            _tr.localRotation = Quaternion.Euler(0, _currentYRotation, 0);
        }
    }
}