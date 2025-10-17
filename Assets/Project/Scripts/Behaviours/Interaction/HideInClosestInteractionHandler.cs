using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class HideInClosestInteractionHandler : MonoInteractableHandlerBase
    {
        public MonoPathAnimation monoPathAnimation;

        public override void HandleInteract(InteractionContext context)
        {
            var character = context.Interactor.GetComponent<MonoCharacterController>();

            // TODO: Убрать каки
            monoPathAnimation.PlayReverse = false;

            var scope = character.PlayerAnimationWithTransform(monoPathAnimation, true);
            scope.Completed += () => character.TryFireTrigger(CharacterStateMachine.Trigger.HidingStarted);
        }
    }

    [InteractableComponent]
    public class ExitOutClosestInteractionHandler : MonoInteractableHandlerBase
    {
        public MonoPathAnimation monoPathAnimation;

        public override void HandleInteract(InteractionContext context)
        {
            var character = context.Interactor.GetComponent<MonoCharacterController>();
            if (character.State != CharacterStateMachine.State.Hiding) return;

            // TODO: Убрать каки
            monoPathAnimation.PlayReverse = true;

            var scope = character.PlayerAnimationWithTransform(monoPathAnimation, true);
            scope.Completed += () => character.TryFireTrigger(CharacterStateMachine.Trigger.HidingEnded);
        }
    }
}