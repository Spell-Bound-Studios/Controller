using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    public class CameraTypeBehaviour : MonoBehaviour {
        [SerializeField] private Helper.CameraType cameraType;
        public Helper.CameraType CameraType => cameraType;
    }
}