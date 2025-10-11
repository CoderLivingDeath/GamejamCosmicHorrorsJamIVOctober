using UnityEngine;
using Zenject;

[InteractableComponent]
public class ShowDialoguePopupInteractableionHandler : InteractableHandlerBehaviourBase
{
    public string DialogId => _dialogId;

    public bool CanInteract = true;

    [SerializeField]
    private string _dialogId;

    [Inject]
    private ViewManager _viewManager;

    [Inject]
    private DialoguePopupView.Factory _factory;

    [Inject]
    private DialogueManager _dialogueManager;

    public override void HandleInteract(InteractionContext context)
    {
        if (!CanInteract) return;

        CanInteract = false;
        
        if (_viewManager == null)
        {
            Debug.LogError("ShowDialoguePopupInteractableionHandler: ViewManager is not assigned.");
            return;
        }

        if (_factory == null)
        {
            Debug.LogError("ShowDialoguePopupInteractableionHandler: Factory is not assigned.");
            return;
        }

        if (!_dialogueManager.TryGetDialog(_dialogId, out var log))
        {
            Debug.LogError($"ShowDialoguePopupInteractableionHandler: Dialog with ID '{_dialogId}' not found.");
            return;
        }

        _dialogueManager.OpenDialog(log);
    }
}