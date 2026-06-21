// Copyright 2025 Spellbound Studio Inc.

using UnityEngine;

namespace Spellbound.Controller.Samples {
    /// <summary>
    /// Action state entered while aiming (bow/gun): on enter it switches the rig to the aiming
    /// <see cref="CameraProfile"/> — Cinemachine blends your current view in to the zoomed one — and restores the
    /// prior camera on exit. This is how a gameplay feature consumes the camera API: through a state, driven by the
    /// same machine that owns the player, not a standalone component bolted onto the scene.
    /// </summary>
    [CreateAssetMenu(fileName = "AimingActionStateExample",
        menuName = "Spellbound/StateMachine/AimingActionStateExample")]
    public class AimingActionStateExample : BaseActionStateExample {
        [SerializeField, Tooltip("Camera switched to while aiming (e.g. the zoomed-in AimingCameraProfile).")]
        private CameraProfile aimProfile;

        private CameraProfile _restore;

        protected override void EnterStateLogic() {
            var rig = CameraRigManager.Instance;

            if (rig == null)
                return;

            _restore = rig.Current;
            rig.Switch(aimProfile);
        }

        protected override void UpdateStateLogic() {
            if (!Ctx.ExampleInput.IsAiming)
                Ctx.ActionStateMachine.ChangeState(ActionStateTypes.Ready);
        }

        protected override void FixedUpdateStateLogic() { }

        protected override void ExitStateLogic() {
            if (_restore != null)
                CameraRigManager.Instance.Switch(_restore);
        }
    }
}
