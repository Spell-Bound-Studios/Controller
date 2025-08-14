using System;
using Unity.Cinemachine;
using UnityEngine;
using Helper = SpellBound.Controller.Configuration.ControllerHelper;

namespace SpellBound.Controller.Configuration {
    public class CameraRigManager : CinemachineCameraManagerBase {
        private CinemachineVirtualCameraBase _freeCamera;
        private CinemachineVirtualCameraBase _zoomCamera;
        
        public CinemachineCamera currentCamera;
        
        protected override void Start() {
            base.Start();

            foreach (var cam in ChildCameras) {
                var camBehaviour = cam.GetComponent<CameraTypeBehaviour>();
                
                if (!camBehaviour)
                    Debug.LogError("Found a camera without a camera type behaviour.", this);
                
                switch (camBehaviour.CameraType) {
                    case Helper.CameraType.Default:
                        _freeCamera = cam;
                        continue;
                    case Helper.CameraType.Zoomed:
                        _zoomCamera = cam;
                        continue;
                    case Helper.CameraType.BirdsEye:
                    case Helper.CameraType.Vehicle:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            currentCamera = (CinemachineCamera)_freeCamera;
        }

        protected override void Update() {
            base.Update();
            
            if (Input.GetKeyDown(KeyCode.E)) {
                currentCamera = currentCamera == (CinemachineCamera)_freeCamera
                        ? (CinemachineCamera)_zoomCamera
                        : (CinemachineCamera)_freeCamera;
            }
        }
        
        protected override CinemachineVirtualCameraBase ChooseCurrentCamera(Vector3 worldUp, float deltaTime) {
            return currentCamera;
        }
    }
}