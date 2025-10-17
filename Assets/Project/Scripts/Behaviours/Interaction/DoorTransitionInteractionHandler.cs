using UnityEngine;
using Zenject;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class DoorTransitionInteractionHandler : MonoInteractableHandlerBase
    {
        [SerializeField]
        private bool PathAnimationReverce;

        [SerializeField]
        public bool NeedKey = false;

        [SerializeField]
        private string ChackPlayerPrefsKey = "Key_taked";

        [SerializeField]
        private MonoDoorTransition doorTransitionAnimation;
        
        [SerializeField]
        private string[] NextId;

        [SerializeField]
        private string[] PrevId;

        [Inject]
        private LocationController locationController;

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

            var scope = doorTransitionAnimation.Animate(monoCharacterController, PathAnimationReverce);
            scope.Completed += () => locationController.HideLocations(PrevId);

            locationController.ActivateRangeLocation(NextId);
        }
    }
}