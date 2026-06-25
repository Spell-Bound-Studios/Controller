// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Action state entered while aiming (bow/gun): on enter it switches the rig to the aiming camera (by name) —
    /// Cinemachine blends your current view in to the zoomed one — and restores the prior camera on exit. This is
    /// how a gameplay feature consumes the camera API: through a state, driven by the same machine that owns the
    /// player, not a standalone component bolted onto the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "AimingActionStateExample",
        menuName = "Spellbound/StateMachine/AimingActionStateExample")]
    public class AimingActionStateExample : BaseActionStateExample {
        [SerializeField, Tooltip("Name of the camera switched to while aiming (the rig camera's GameObject name).")]
        private string aimCameraName = "AimingCamera";

        [Header("Animation")]
        [SerializeField, Tooltip("Animator state on the masked action layer played while aiming.")]
        private string aimStateName = "Aiming";
        private int _aimHash;

        private string _restore;

        protected override void OnStateInitialize() => _aimHash = Animator.StringToHash(aimStateName);

        protected override void EnterStateLogic() {
            Ctx.Animation?.CrossFade(_aimHash, PlayerControllerExample.ActionLayer);
            Ctx.FadeActionLayer(1f);

            var rig = CameraRigManager.Instance;

            if (rig == null)
                return;

            _restore = rig.Current;
            rig.Switch(aimCameraName);
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.ExampleInput.IsAiming)
                Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Ready);
        }

        protected override void FixedUpdateStateLogic() { }

        protected override void ExitStateLogic() {
            Ctx.FadeActionLayer(0f);

            if (!string.IsNullOrEmpty(_restore))
                CameraRigManager.Instance.Switch(_restore);
        }
    }
}
