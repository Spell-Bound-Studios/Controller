using System;
using UnityEngine;

namespace SpellBound.Controller.ManagersAndStatics {
    /// <summary>
    /// This static class is meant to contain optional helpers that a user might want to use as a reference or directly
    /// call. Please note: you cannot override a static method, and therefore, if you're attempting to create state
    /// variants that override a specific behavior, then you will need to create a protected abstract or virtual method
    /// in the base state and either implement or wrap one of these methods.
    /// </summary>
    public static class ControllerHelper {
        public enum CameraCouplingMode {
            Coupled, 
            CoupledWhenMoving, 
            Decoupled
        }

        public enum CameraType {
            Default,
            Zoomed,
            BirdsEye,
            Vehicle
        }

        public enum CastDirection {
            Forward,
            Backward,
            Left,
            Right,
            Up,
            Down
        }

        public enum RaycastLength {
            Normal,
            Extended,
            Retracted
        }

        /// <summary>
        /// Returns the horizontal velocity of a given rigidbody.
        /// </summary>
        public static Vector3 GetHorizontalVelocity(Rigidbody rb) {
            var velocity = rb.linearVelocity;
            velocity.y = 0;
            return velocity;
        }
        
        /// <summary>
        /// Returns the vertical speed relative to the specified up direction.
        /// </summary>
        public static float GetVerticalSpeed(Rigidbody rb, Vector3 upDirection) {
            return Vector3.Dot(rb.linearVelocity, upDirection);
        }
        
        /// <summary>
        /// Returns the planar velocity projected onto the specified plane.
        /// </summary>
        public static Vector3 GetPlanarVelocity(Rigidbody rb, Vector3 planeNormal) {
            return Vector3.ProjectOnPlane(rb.linearVelocity, planeNormal);
        }
        
        /// <summary>
        /// Returns the current speed of the rigidbody.
        /// </summary>
        public static float GetCurrentSpeed(Rigidbody rb) {
            return rb.linearVelocity.magnitude;
        }
        
        // ########################
        // INPUT PROCESSING HELPERS
        // ########################
        
        /// <summary>
        /// Converts an input direction to world space relative to a reference transform and up direction.
        /// </summary>
        public static Vector3 GetInputDirectionRelativeToCamera(
                Vector2 inputDirection, Transform referenceTransform, Vector3 upDirection) {
            if (referenceTransform == null) 
                return Vector3.zero;
            
            var rightProjected = Vector3.ProjectOnPlane(
                    referenceTransform.right, upDirection).normalized;
            var forwardProjected = Vector3.ProjectOnPlane(
                    referenceTransform.forward, upDirection).normalized;
            
            var direction = rightProjected * inputDirection.x + forwardProjected * inputDirection.y;
            
            return direction.magnitude > 1f 
                    ? direction.normalized 
                    : direction;
        }
        
        /// <summary>
        /// Normalizes an input direction if magnitude exceeds 1 (useful for analog stick input).
        /// </summary>
        public static Vector2 NormalizeInputDirection(Vector2 inputDirection) {
            return inputDirection.magnitude > 1f 
                    ? inputDirection.normalized 
                    : inputDirection;
        }
        
        // #######################
        // GROUND CHECKING HELPERS
        // #######################
        
        /// <summary>
        /// Performs a basic ground check raycast from the specified origin.
        /// Returns true if ground is detected within the specified distance.
        /// </summary>
        public static bool CheckGroundRaycast(
                Vector3 origin, Vector3 downDirection, float maxDistance, LayerMask groundLayers, out RaycastHit hitInfo) {
            
            return Physics.Raycast(
                origin: origin,
                direction: downDirection,
                hitInfo: out hitInfo,
                maxDistance: maxDistance,
                layerMask: groundLayers,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore
            );
        }
        
        /// <summary>
        /// Checks ground using a sphere cast for more reliable detection.
        /// </summary>
        public static bool CheckGroundSphereCast(
                Vector3 origin, float radius, Vector3 downDirection, float maxDistance, LayerMask groundLayers, out RaycastHit hitInfo) {
            
            return Physics.SphereCast(
                origin: origin,
                radius: radius,
                direction: downDirection,
                hitInfo: out hitInfo,
                maxDistance: maxDistance,
                layerMask: groundLayers,
                queryTriggerInteraction: QueryTriggerInteraction.Ignore
            );
        }
        
        // ################
        // ROTATION HELPERS
        // ################
        
        /// <summary>
        /// Smoothly rotates towards a target direction using the specified rotation speed.
        /// </summary>
        public static Quaternion RotateTowardsDirection(
                Quaternion currentRotation, Vector3 targetDirection, Vector3 upDirection, float rotationSpeed, float deltaTime) {
            
            if (targetDirection.sqrMagnitude < 1e-6f) 
                return currentRotation;
            
            var targetRotation = Quaternion.LookRotation(targetDirection, upDirection);
            var maxRotationStep = rotationSpeed * deltaTime;
            
            return Quaternion.RotateTowards(currentRotation, targetRotation, maxRotationStep);
        }
        
        /// <summary>
        /// Returns the angle difference between current rotation and target direction.
        /// </summary>
        public static float GetRotationAngleDifference(
                Quaternion currentRotation, Vector3 targetDirection, Vector3 upDirection) {
            
            if (targetDirection.sqrMagnitude < 1e-6f) 
                return 0f;
            
            var targetRotation = Quaternion.LookRotation(targetDirection, upDirection);
            return Quaternion.Angle(currentRotation, targetRotation);
        }
        
        // ###############
        // PHYSICS HELPERS
        // ###############
        
        /// <summary>
        /// Handles character rotation with angle-based speed falloff.
        /// This is the preferred rotation method for standard player-based character controllers.
        /// </summary>
        public static void HandleCharacterRotation(
                Rigidbody rb, Vector3 planarUp, float turnSpeed, float rotationFallOffAngle, float deltaTime) {
            
            var planarVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, planarUp);

            if (planarVelocity.sqrMagnitude < 1e-6f)
                return;

            var desiredDir = planarVelocity.normalized;
            var targetRotation = Quaternion.LookRotation(desiredDir, planarUp);
            var angleDiff = Quaternion.Angle(rb.rotation, targetRotation);
            var speedFactor = Mathf.InverseLerp(0f, rotationFallOffAngle, angleDiff);
            
            var maxStepDeg = turnSpeed * speedFactor * deltaTime;

            var nextRotation = Quaternion.RotateTowards(rb.rotation, targetRotation, maxStepDeg);
            
            rb.MoveRotation(nextRotation);
        }
        
        /// <summary>
        /// Applies force to achieve a target horizontal velocity.
        /// </summary>
        public static void ApplyHorizontalMovementForce(
                Rigidbody rb, Vector3 targetVelocity, float forceMultiplier, ForceMode forceMode) {
            
            var currentHorizontalVelocity = GetHorizontalVelocity(rb);
            var velocityDifference = targetVelocity - currentHorizontalVelocity;
            var force = velocityDifference * forceMultiplier;
            
            rb.AddForce(force, forceMode);
        }
        
        /// <summary>
        /// Applies a step-up force to help with ground adherence.
        /// </summary>
        public static void ApplyStepUpForce(Rigidbody rb, float stepForce, Vector3 upDirection, ForceMode forceMode) {
            var upForce = upDirection * stepForce;
            rb.AddForce(upForce, forceMode);
        }
        
        // ###########################
        // COLLISION & OVERLAP HELPERS
        // ###########################
        
        /// <summary>
        /// Checks for overlapping colliders within a specified radius.
        /// </summary>
        public static bool CheckOverlapSphere(Vector3 position, float radius, LayerMask layers) {
            return Physics.CheckSphere(position, radius, layers, QueryTriggerInteraction.Ignore);
        }
        
        /// <summary>
        /// Returns all colliders overlapping with a sphere.
        /// </summary>
        public static Collider[] GetOverlappingColliders(
                Vector3 position, float radius, LayerMask layers, int maxResults = 10) {
            
            var results = new Collider[maxResults];
            var count = Physics.OverlapSphereNonAlloc(position, radius, results, layers, QueryTriggerInteraction.Ignore);
            
            if (count == maxResults) 
                return results;
            
            var trimmedResults = new Collider[count];
            Array.Copy(results, trimmedResults, count);
            return trimmedResults;
        }
        
        // ####################
        // MATHEMATICAL HELPERS
        // ####################
        
        /// <summary>
        /// Lerps between two values with a speed-based approach rather than time-based.
        /// </summary>
        public static float LerpWithSpeed(float current, float target, float speed, float deltaTime) {
            var maxChange = speed * deltaTime;
            var difference = target - current;
            var clampedChange = Mathf.Clamp(difference, -maxChange, maxChange);
            return current + clampedChange;
        }
        
        /// <summary>
        /// Smoothly damps a value towards a target using SmoothDamp.
        /// </summary>
        public static float SmoothDampFloat(
                float current, float target, ref float velocity, float smoothTime, float deltaTime, float maxSpeed = Mathf.Infinity) {
            return Mathf.SmoothDamp(current, target, ref velocity, smoothTime, maxSpeed, deltaTime);
        }
        
        /// <summary>
        /// Maps a value from one range to another.
        /// </summary>
        public static float RemapValue(float value, float fromMin, float fromMax, float toMin, float toMax) {
            var normalizedValue = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, normalizedValue);
        }
    }
}