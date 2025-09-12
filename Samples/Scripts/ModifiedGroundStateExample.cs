using UnityEngine;

namespace SpellBound.Controller.Samples {
    [CreateAssetMenu(fileName = "ModifiedGroundStateExample", menuName = "Spellbound/StateMachine/ModifiedGroundStateExample")]
    public class ModifiedGroundStateExample : GroundStateExample {
        protected override void UpdateStateLogic() {
            Debug.Log("Greetings!");
        }
    }
}