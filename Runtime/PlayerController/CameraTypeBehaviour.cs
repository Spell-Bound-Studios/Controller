using UnityEngine;
using Helper = SpellBound.Controller.ManagersAndStatics.ControllerHelper;

namespace SpellBound.Controller.PlayerController {
    public class CameraTypeBehaviour : MonoBehaviour {
        [SerializeField] private Helper.CameraType cameraType;
        public Helper.CameraType CameraType => cameraType;
    }
}