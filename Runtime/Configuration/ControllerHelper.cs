using UnityEngine;

namespace SpellBound.Controller.Configuration {
    public static class ControllerHelper {
        public const float YAngleMax = 90;
        public const float YAngleMin = -90;
        
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

        /// <summary>
        /// From git-amends channel and returns a component of a vector that is in the direction of a given vector.
        /// </summary>
        public static Vector3 ExtractDotVector(Vector3 vector, Vector3 direction) {
            direction.Normalize();
            return direction * Vector3.Dot(vector, direction);
        }

        /// <summary>
        /// From git-amends channel and removes the component of a vector that is in the direction of the given vector.
        /// </summary>
        public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction) {
            direction.Normalize();
            return vector - direction * Vector3.Dot(vector, direction);
        }

        /// <summary>
        /// Calculates the signed angle between two vectors on a plane defined by a normal vector.
        /// </summary>
        public static float GetAngle(Vector3 vector1, Vector3 vector2, Vector3 planeNormal) {
            var angle = Vector3.Angle(vector1, vector2);
            var sign = Mathf.Sign(Vector3.Dot(planeNormal, Vector3.Cross(vector1, vector2)));
            return angle * sign;
        }
    }
}