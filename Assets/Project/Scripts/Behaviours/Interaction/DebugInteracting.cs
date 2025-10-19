using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class DebugInteracting : MonoInteractableHandlerBase
    {
        public string Message = "Message";
        public override void HandleInteract(InteractionContext context)
        {
            Debug.Log(this.ToString() + " has interacted." + "Message: " + Message);
        }
    }
}
