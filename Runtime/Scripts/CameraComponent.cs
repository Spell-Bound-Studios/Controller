using UnityEngine;
using UnityEngine.Serialization;

namespace SpellBound.Controller {
    public class CameraComponent : MonoBehaviour {
        public Camera Camera { get; private set; }

        [Header("Camera Settings")] 
        [SerializeField] private float camTargetHeight = 1.5f;
        [SerializeField] private float camTargetOffset = 0.3f;
        [SerializeField] private float camCollisionPullForwardDistance = 1;
        private Vector3 _camTarget;
        [SerializeField] private float mouseSensitivity = 7f;
        [SerializeField] private float cameraDistance = 5f;
        private Vector3 _cameraVelocity;
        private const float CameraSmoothSpeed = 1f;
        private float _yaw;
        private float _pitch;
        private const float MinimumPivot = -30f;
        private const float MaximumPivot = 89f;
        private Vector3 _cameraShovedPosition;
        
        // Allows outward methods to turn on or off if the camera follows the mouse.
        public bool camFollowMouse = true;
        
        [Header("Camera Collision Values")]
        [SerializeField] private LayerMask collisionLayerMask = ~0;
        [Header("Debugging")]
        [SerializeField] private bool drawCollisionDebugGizmos;
        
        private void Awake() {
            Debug.Log("Camera Component is Awake!");
            GetCamera();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void FixedUpdate() {
            HandleCameraTransform(camFollowMouse);
        }
        
        /// <summary>
        /// Late Update method that polls how the camera should rotate with the mouse about a target transform and follow it.
        /// </summary>
        private void HandleCameraTransform(bool followMouse) {
            if (followMouse) {
                var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
                var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
                _yaw += mouseX;
                _pitch -= mouseY;
                _pitch = Mathf.Clamp(_pitch, MinimumPivot, MaximumPivot);
            }
            
            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            _camTarget = transform.position + Vector3.up * camTargetHeight + rotation * (Vector3.right + Vector3.up) * camTargetOffset;
            Vector3 targetPosition;
            
            if (Physics.Raycast(_camTarget, rotation * Vector3.back, out RaycastHit hit, cameraDistance, collisionLayerMask)) {
                targetPosition = hit.point + rotation * Vector3.forward * camCollisionPullForwardDistance;
            }
            else targetPosition = _camTarget + rotation * new Vector3(0, 0, -cameraDistance);
            
            Camera.transform.position = Vector3.SmoothDamp(
                Camera.transform.position,
                targetPosition,
                ref _cameraVelocity,
                CameraSmoothSpeed * Time.deltaTime
            );

            Camera.transform.LookAt(_camTarget);
        }
        
        /// <summary>
        /// Helper function for getting the camera component or creating it.
        /// </summary>
        private void GetCamera() {
            if (Camera != null) return;
            // TODO: Maybe come up with a better solution than a tag. Low priority.
            Camera = GameObject.FindWithTag("Player")?.GetComponent<Camera>();
            Debug.Log(Camera.name);
            if (Camera != null) return;
            var cameraGameObject = new GameObject("Main Camera");
            Camera = cameraGameObject.AddComponent<Camera>();
            Camera.tag = "Player";
            Camera.rect = new Rect(0.8f, 0.8f, 0.2f, 0.2f);
        }
        
        public void AdjustMouseSensitivity(float newValue) {
            mouseSensitivity = newValue;
        }

        public float GetMouseSensitivity() {
            return mouseSensitivity;
        }
        
        /// <summary>
        /// Debugging Gizmo
        /// </summary>
        private void OnDrawGizmos() {
            if (!drawCollisionDebugGizmos) return;
            if (Camera == null) return;

            var start = _camTarget;
            var direction = Camera.transform.position - start;
            var distance = direction.magnitude;

            direction.Normalize();

            Gizmos.color = Color.red;
            Gizmos.DrawRay(start, direction * distance);

            Gizmos.color = Color.green;
        }
    }
}