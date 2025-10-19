using UnityEngine;
using Zenject;

namespace Project.Scripts.behaviours.Interaction.InteractableHandlers
{
    [InteractableComponent]
    public class PuzzleStartedInteractionHandler : MonoInteractableHandlerBase
    {

        [SerializeField]
        private bool CanInteract = true;

        [SerializeField]
        private GameObject monoCanvasView;

        [Inject]
        private ViewManager viewManager;

        public override void HandleInteract(InteractionContext context)
        {
            if (CanInteract)
            {
                var view = viewManager.InstantiateViewFromPrefab(monoCanvasView);
                view.OnClosed += () => CanInteract = true;
                
                CanInteract = false;
            }
        }
    }
}