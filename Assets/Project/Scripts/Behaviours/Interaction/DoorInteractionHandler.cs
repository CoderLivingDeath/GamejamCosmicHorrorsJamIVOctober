using UnityEngine;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class DoorInteractionHandler : MonoInteractableHandlerBase
    {
        [SerializeField]
        private bool PathAnimationReverce;

        [SerializeField]
        public bool NeedKey = false;

        [SerializeField]
        private string ChackPlayerPrefsKey = "Key_taked";

        [SerializeField]
        private DoorTransitionAnimation doorTransitionAnimation;
        public override void HandleInteract(InteractionContext context)
        {
            if (NeedKey)
            {
                if (PlayerPrefs.GetInt(ChackPlayerPrefsKey) != 1)
                {
                    Debug.Log("Key not taked.");
                    return;
                }
            }
            MonoCharacterController monoCharacterController = context.Interactor.GetComponent<MonoCharacterController>();

            doorTransitionAnimation.Animate(monoCharacterController, PathAnimationReverce);
        }
    }
}