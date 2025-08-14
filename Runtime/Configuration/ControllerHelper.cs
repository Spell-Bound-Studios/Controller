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
    }
}