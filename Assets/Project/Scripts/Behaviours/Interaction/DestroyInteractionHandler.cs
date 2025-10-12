[InteractableComponent]
public class DestroyInteractionHandler : MonoInteractableHandlerBase
{
    public override void HandleInteract(InteractionContext context)
    {
        Destroy(gameObject);
    }
}