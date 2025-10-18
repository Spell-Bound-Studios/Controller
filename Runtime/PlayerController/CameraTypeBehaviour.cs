// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller {
    public class CameraTypeBehaviour : MonoBehaviour {
        [SerializeField] private ControllerHelper.CameraType cameraType;
        public ControllerHelper.CameraType CameraType => cameraType;
    }
}