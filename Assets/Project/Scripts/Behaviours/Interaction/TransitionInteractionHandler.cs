using UnityEngine;

[InteractableComponent]
public class TransitionInteractionHandler : InteractableHandlerBehaviourBase
{
    public Transform TransitionalPosition;

    public override void HandleInteract(InteractionContext context)
    {
        if(context.Interactor.TryGetComponent<MovementBehaviour>(out var component))
        {
            component.Teleport(TransitionalPosition.position);
        }
    }
}