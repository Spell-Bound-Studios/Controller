// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Action state entered while aiming (bow/gun): it eases the live camera in over the shoulder via
    /// <see cref="AimZoomExample"/> and eases back out on exit. The view direction never changes — aiming
    /// zooms in on exactly what the crosshair was on.
    /// </summary>
    [CreateAssetMenu(fileName = "AimingActionStateExample",
        menuName = "Spellbound/StateMachine/AimingActionStateExample")]
    public class AimingActionStateExample : BaseActionStateExample {
        [Header("Animation")]
        [SerializeField, Tooltip("Animator state on the masked action layer played while aiming.")]
        private string aimStateName = "Aiming";
        private int _aimHash;

        private AimZoomExample _zoom;

        protected override void OnStateInitialize() => _aimHash = Animator.StringToHash(aimStateName);

        protected override void EnterStateLogic() {
            Ctx.Animation?.CrossFade(_aimHash, PlayerControllerExample.ActionLayer);
            Ctx.FadeActionLayer(1f);

            _zoom = FindAimZoom();

            if (_zoom != null)
                _zoom.Aiming = true;
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.ExampleInput.IsAiming)
                Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Ready);
        }

        protected override void FixedUpdateStateLogic() { }

        protected override void ExitStateLogic() {
            Ctx.FadeActionLayer(0f);

            if (_zoom != null) {
                _zoom.Aiming = false;
                _zoom = null;
            }
        }

        private AimZoomExample FindAimZoom() {
            var cameraTransform = Ctx.CameraRig?.CurrentCameraTransform;

            return cameraTransform != null
                    ? cameraTransform.GetComponent<AimZoomExample>()
                    : null;
        }
    }
}
