using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class TakeKeyInteractionHanlder : MonoInteractableHandlerBase
    {
        public string KeyId = "Key_taked";
        public override void HandleInteract(InteractionContext context)
        {
            Debug.Log("Key taked:" + 1);
            PlayerPrefs.SetInt(KeyId, 1);
        }
    }
}