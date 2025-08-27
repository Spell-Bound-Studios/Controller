using System.Collections;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "GCDState", menuName = "Spellbound/ActionStates/GCDState")]
    public class GCDStateSO : BaseActionStateSO {
        private const float DefaultDelay = 0.5f;
        
        [SerializeField] private float delay;
        [SerializeField] private StateHelper.States defaultGcdAnimation;

        private WaitForSeconds _gcdDelay;
        private Coroutine _gcdRoutine;
        
        public override void EnterStateLogic(ActionStateMachine stateMachine) {
            StateMachine = stateMachine;

            StateHelper.NotifyActionStateChange(this);
            
            _gcdDelay = new WaitForSeconds(DefaultDelay);
            _gcdRoutine = Cc.StartCoroutine(GCDRoutine());
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() {
            
        }
        
        public override void CheckSwitchStateLogic() {
            if (_gcdRoutine == null)
                StateMachine.ChangeState(StateMachine.ReadyStateDriver);
        }
        
        public override void ExitStateLogic() {
            if (_gcdRoutine != null) 
                Cc.StopCoroutine(_gcdRoutine);

            Cc.hotKeyOnePressed = false;
            Cc.interactKeyPressed = false;
        }
        
        private IEnumerator GCDRoutine() {
            yield return _gcdDelay;
            _gcdRoutine = null;
        }
    }
}