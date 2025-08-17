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
        private float _currentYRotation;
        private const float FallOffAngle = 90f;

        private void Awake() {
            _tr = transform;
        }
        
        private void Start() {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();

            if (referenceTransform == null) {
                Debug.LogWarning("[PlayerFaceInputDirection] ReferenceTransform is null, using default reference.");
                referenceTransform = _tr;
            }
            
            _currentYRotation = _tr.eulerAngles.y;
        }

        private void LateUpdate() {
            // Basically gives the x,z components of our velocity vector since they are normal to the up direction.
            var velocity = Vector3.ProjectOnPlane(
                    playerController.GetMovementVelocity(), referenceTransform.up);
            
            // Return early if we're not moving.
            if (velocity.magnitude < 0.001f)
                return;
            
            var camForwardPlanar = Vector3.ProjectOnPlane(
                    referenceTransform.forward, referenceTransform.up);

            if (camForwardPlanar.magnitude < 0.001f)
                camForwardPlanar = Vector3.ProjectOnPlane(
                        _tr.forward, referenceTransform.up).normalized;
            
            var camRightPlanar = Vector3.Cross(referenceTransform.up, camForwardPlanar);
            
            var forward = Vector3.Dot(velocity, camForwardPlanar);
            var right = Vector3.Dot(velocity, camRightPlanar);
            
            var desiredFacingDir = Mathf.Abs(right) > Mathf.Abs(forward)
                    ? right >= 0f ? camRightPlanar : -camRightPlanar
                    : forward >= 0f ? camForwardPlanar : -camForwardPlanar;
            
            var angleDiff = Helper.GetAngle(_tr.forward, desiredFacingDir, referenceTransform.up);
            //var angleDiff = Helper.GetAngle(referenceTransform.forward, velocity.normalized, referenceTransform.up);

            var step = Mathf.Sign(angleDiff) *
                       Mathf.InverseLerp(0f, FallOffAngle, Mathf.Abs(angleDiff)) *
                       Time.deltaTime * turnSpeed;
            
            _currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDiff) ? angleDiff : step;
            
            _tr.localRotation = Quaternion.Euler(0, _currentYRotation, 0);
        }
    }
}