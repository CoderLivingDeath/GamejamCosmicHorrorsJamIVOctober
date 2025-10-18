using System;
using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class HideInClosestInteractionHandler : MonoInteractableHandlerBase
    {

        [SerializeField]
        private MonoHideScript monoHideInClosest;

        public override void HandleInteract(InteractionContext context)
        {
            var character = context.Interactor.GetComponent<MonoCharacterController>();
            monoHideInClosest.SwitchHide(character);
        }
    }
}