using UnityEngine;

public abstract class MonoInteractableHandlerBase : MonoBehaviour
{
    public virtual void HandleInteract(InteractionContext context)
    {
        // code
        Debug.LogWarning($"Interaction handler is not defined.");
    }
}