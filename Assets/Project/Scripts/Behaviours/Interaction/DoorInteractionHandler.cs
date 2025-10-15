using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class DoorInteractionHandler : MonoInteractableHandlerBase
    {
        [SerializeField]
        private bool PathAnimationReverce;

        [SerializeField]
        private DoorTransitionAnimation doorTransitionAnimation;
        public override void HandleInteract(InteractionContext context)
        {
            MonoCharacterController monoCharacterController = context.Interactor.GetComponent<MonoCharacterController>();

            doorTransitionAnimation.Animate(monoCharacterController, PathAnimationReverce);
        }
    }
}