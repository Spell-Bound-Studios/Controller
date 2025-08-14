using Unity.Cinemachine;
using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    public class CameraController : MonoBehaviour {
        [SerializeField] private CinemachineCameraManagerBase cameraRig;
        [SerializeField] private Helper.CameraCouplingMode playerRotationMode;
        private CinemachineBrain _brain;

        private void Awake() {
            if (!_brain && Camera.main) 
                Camera.main.TryGetComponent(out _brain);
        }
        
        private void Start() {
            CameraSetup();
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        private void CameraSetup() {
            cameraRig = _brain.ActiveVirtualCamera as CinemachineCameraManagerBase;

            if (cameraRig == null) {
                Debug.LogError("Camera rig is null and doesn't appear to be in scene.");
                return;
            }

            cameraRig.DefaultTarget.Target.TrackingTarget = transform;
        }
    }
}