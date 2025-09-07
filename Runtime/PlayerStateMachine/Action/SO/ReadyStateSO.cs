using SpellBound.Core;
using SpellBound.CorsairsWorld;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;

namespace SpellBound.Controller.PlayerStateMachine {
    [CreateAssetMenu(fileName = "ReadyState", menuName = "Spellbound/ActionStates/ReadyState")]
    public class ReadyStateSO : BaseActionStateSO {
        public override void EnterStateLogic(ActionStateMachine stateMachine) {
            StateMachine = stateMachine;
            
            StateHelper.NotifyActionStateChange(this);
            Cc.input.OnHotkeyOnePressed += HandleHotkeyOnePressed;
            Cc.input.OnInteractPressed += TemporaryEcsBandAidInteractPressed;
        }
        
        public override void UpdateStateLogic() {
            CheckSwitchStateLogic();
        }
        
        public override void FixedUpdateStateLogic() { }
        
        public override void CheckSwitchStateLogic() { }

        public override void ExitStateLogic() {
            Cc.input.OnHotkeyOnePressed -= HandleHotkeyOnePressed;
            Cc.input.OnInteractPressed -= TemporaryEcsBandAidInteractPressed;
        }

        private void HandleHotkeyOnePressed() {
            if (!Physics.Raycast(
                        Cc.referenceTransform.position,
                        Cc.referenceTransform.forward,
                        out var hit,
                        10f,
                        1 << 6
                ))
                return;
            
            ClientChunkManager.Instance.DigSphere(hit.point, 2f, -255);
            StateMachine.ChangeState(StateMachine.GCDStateDriver);
        }
        
        /// <summary>
        /// This is a placeholder method for once we solve ECS/Gameobject interoperability.
        /// </summary>
        private void TemporaryEcsBandAidInteractPressed() {
            var refTr = Cc.GetReferenceTransform();
            
            // Do ECS raycast here.
            var start = refTr.position;
            var direction = refTr.forward;
            var end = start + direction * (10f); // Magic numbers don't matter. This method is temporary.

            // Get the ECS physics world.
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            var physicsWorldSingleton = entityManager
                .CreateEntityQuery(typeof(PhysicsWorldSingleton))
                .GetSingleton<PhysicsWorldSingleton>();

            var physicsWorld = physicsWorldSingleton.PhysicsWorld;

            // Set up the raycast input.
            var rayInput = new RaycastInput {
                Start = start,
                End = end,
                Filter = new CollisionFilter {
                    BelongsTo = ~0u, // Ray belongs to all categories
                    CollidesWith = 1 << 18, // Collides with specified layers
                    GroupIndex = 0
                }
            };

            // Perform the raycast and log if something was hit.
            if (physicsWorld.CollisionWorld.CastRay(rayInput, out var hitEcs)) {
                Debug.Log($"[ECS] Raycast hit Entity: {hitEcs.Entity.Index} at position {hitEcs.Position}");
                var sbb = SpellBoundSwapManager.InteractionSwap(hitEcs.Entity);

                if (!sbb)
                    return;
                
                if (sbb.TryGetComponent<IInteractable>(out var swappedTarget)) {
                    swappedTarget.Interact(Cc.gameObject);
                    StateMachine.ChangeState(StateMachine.InteractStateDriver);
                    return;
                }
            }
            
            if (!Physics.Raycast(
                        refTr.position,
                    refTr.forward,
                    out var hit,
                    10f,
                    1 << 18
                ))
                return;
            
            if (!hit.collider.TryGetComponent<IInteractable>(out var target)) 
                return;
            
            target.Interact(Cc.gameObject);
            StateMachine.ChangeState(StateMachine.InteractStateDriver);
        }
    }
}