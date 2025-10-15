using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class TakeKeyInteractionHanlder : MonoInteractableHandlerBase
    {
        public override void HandleInteract(InteractionContext context)
        {
            Debug.Log("Key taked:" + 1);
            PlayerPrefs.SetInt("Key_taked", 1);
        }
    }
}