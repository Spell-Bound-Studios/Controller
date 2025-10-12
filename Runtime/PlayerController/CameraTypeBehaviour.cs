// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;
using Helper = SpellBound.Controller.ControllerHelper;

namespace SpellBound.Controller {
    public class CameraTypeBehaviour : MonoBehaviour {
        [SerializeField] private Helper.CameraType cameraType;
        public Helper.CameraType CameraType => cameraType;
    }
}