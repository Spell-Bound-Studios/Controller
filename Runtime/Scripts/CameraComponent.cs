using UnityEngine;

namespace SpellBound.Controller {
    public class CameraComponent : MonoBehaviour {
        public Camera Camera { get; private set; }

        [Header("Camera Settings")] 
        [SerializeField] private Vector3 cameraFollowOffset = new (0.25f, 1.5f, 0f);

        [SerializeField] private Vector3 camTarget;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float cameraDistance = 5f;
        private Vector3 _cameraVelocity;
        private const float CameraSmoothSpeed = 1f;
        private float _yaw;
        private float _pitch;
        private const float MinimumPivot = -30f;
        private const float MaximumPivot = 60f;
        private Vector3 _cameraShovedPosition;
        
        [Header("Camera Collision Values")]
        [SerializeField] private LayerMask collisionLayerMask;
        [SerializeField] private float targetCameraZPosition;
        [SerializeField] private float cameraCollisionRadius = 0.2f;
        [SerializeField] private float cameraShoveVelocity = 0.2f;
        
        [Header("Debugging")]
        [SerializeField] private bool drawCollisionDebugGizmos;
        
        private void Awake() {
            GetCamera();
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void LateUpdate() {
            HandleCameraTarget();
            HandleCameraRotation();
            HandleCameraCollision();
        }
        
        /// <summary>
        /// Late Update method that polls how the camera should rotate with the mouse about a target transform and follow it.
        /// </summary>
        private void HandleCameraRotation() {
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            _yaw += mouseX;
            _pitch -= mouseY;
            _pitch = Mathf.Clamp(_pitch, MinimumPivot, MaximumPivot);

            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var rotatedOffset = rotation * new Vector3(0, 0, -cameraDistance);
            var targetPosition = camTarget + rotatedOffset;

            Camera.transform.position = Vector3.SmoothDamp(
                Camera.transform.position,
                targetPosition,
                ref _cameraVelocity,
                CameraSmoothSpeed * Time.deltaTime
            );

            Camera.transform.LookAt(camTarget);
        }
        
        /// <summary>
        /// Late Update method that polls how the camera should handle collisions in the game world.
        /// BUG: Forces the camera down and has odd jittery behavior.
        /// </summary>
        private void HandleCameraCollision() {
            targetCameraZPosition = Vector3.Distance(cameraFollowOffset, Camera.transform.position);
            
            var direction = Camera.transform.position - cameraFollowOffset;
            direction.Normalize();

            if (!Physics.SphereCast(
                    cameraFollowOffset,
                    cameraCollisionRadius,
                    direction,
                    out var hit,
                    Mathf.Abs(targetCameraZPosition),
                    collisionLayerMask
                )) return;

            var distanceFromHitObject = Vector3.Distance(cameraFollowOffset, hit.point);
            targetCameraZPosition = -(distanceFromHitObject - cameraCollisionRadius);

            if (Mathf.Abs(targetCameraZPosition) < cameraCollisionRadius) {
                targetCameraZPosition = -cameraCollisionRadius;
            }
            
            _cameraShovedPosition.z = Mathf.Lerp(Camera.transform.localPosition.z, targetCameraZPosition, cameraShoveVelocity);
            Camera.transform.localPosition = _cameraShovedPosition;
        }
        
        /// <summary>
        /// Helper function for getting the camera component or creating it.
        /// </summary>
        private void GetCamera() {
            if (Camera != null) return;
            Camera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
            
            if (Camera != null) return;
            
            var cameraGameObject = new GameObject("Main Camera");
            Camera = cameraGameObject.AddComponent<Camera>();
            Camera.tag = "MainCamera";
        }
        
        /// <summary>
        /// Handles the camera target and ensures it stays at the same point in space.
        /// </summary>
        private void HandleCameraTarget() {
            camTarget = transform.position + (Camera.transform.rotation * cameraFollowOffset);
        }
        
        /// <summary>
        /// Debugging Gizmo
        /// </summary>
        private void OnDrawGizmos() {
            if (!drawCollisionDebugGizmos) return;
            if (Camera == null) return;

            var start = cameraFollowOffset;
            var direction = Camera.transform.position - start;
            var distance = direction.magnitude;

            direction.Normalize();

            Gizmos.color = Color.red;
            Gizmos.DrawRay(start, direction * distance);

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(start + direction * distance, cameraCollisionRadius);
        }
    }
}