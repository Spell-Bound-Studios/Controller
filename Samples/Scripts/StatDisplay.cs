using SpellBound.Controller.PlayerController;
using TMPro;
using UnityEngine;

namespace SpellBound.Controller.Samples {
    public class StatDisplay : MonoBehaviour {
        private RigidbodyMover _rbm;
        [SerializeField] private TMP_Text horizontalSpeed;
        [SerializeField] private TMP_Text verticalSpeed;

        private void Awake() {
            if (horizontalSpeed == null || verticalSpeed == null)
                Debug.LogError("Please drag and drop the TMP_Text components into the speed field for StatDisplay.", 
                        this);
        }

        /// <summary>
        /// This is bad code. This only for the example scene where performance doesn't matter.
        /// </summary>
        private void FixedUpdate() {
            if (_rbm == null) {
                _rbm = FindFirstObjectByType<RigidbodyMover>();
                return;
            }

            var vel = _rbm.GetRigidbodyVelocity();
            var up = _rbm.transform.up;
            var vertical = Vector3.Dot(vel, up);
            var horizontal = Vector3.ProjectOnPlane(vel, up).magnitude;
            
            verticalSpeed.text = $"Vertical Speed: {vertical:F2}";
            horizontalSpeed.text = $"Horizontal Speed: {horizontal:F2}";
        }
    }
}