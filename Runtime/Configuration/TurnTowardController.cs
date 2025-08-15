using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    [RequireComponent(typeof(PlayerController))]
    public class TurnTowardController : MonoBehaviour {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private float turnSpeed = 50f;

        private Transform _tr;
        private float _currentYRotation;
        private const float FallOffAngle = 90f;

        private void Awake() {
            _tr = transform;
        }
        private void Start() {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            
            _currentYRotation = _tr.eulerAngles.y;
        }

        private void LateUpdate() {
            var velocity = Vector3.ProjectOnPlane(playerController.GetMovementVelocity(), _tr.up);
            
            if (velocity.magnitude < 0.001f)
                return;

            var angleDiff = Helper.GetAngle(_tr.forward, velocity.normalized, _tr.up);

            var step = Mathf.Sign(angleDiff) *
                       Mathf.InverseLerp(0f, FallOffAngle, Mathf.Abs(angleDiff)) *
                       Time.deltaTime * turnSpeed;
            
            _currentYRotation += Mathf.Abs(step) > Mathf.Abs(angleDiff) ? angleDiff : step;
            
            _tr.localRotation = Quaternion.Euler(0, _currentYRotation, 0);
        }
    }
}