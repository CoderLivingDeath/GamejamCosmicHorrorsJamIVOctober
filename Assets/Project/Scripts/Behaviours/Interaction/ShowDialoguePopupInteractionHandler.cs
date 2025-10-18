using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

[InteractableComponent]
public class ShowDialoguePopupInteractableionHandler : MonoInteractableHandlerBase
{
    [Serializable]
    private struct OnNextDialogueContext
    {
        public int DialogueId;
        public UnityEvent On;
    }
    public string DialogId => _dialogId;

    public bool CanInteract = true;

    [SerializeField]
    private string _dialogId;

    [SerializeField]
    private UnityEvent OnStart;

    [SerializeField]
    private UnityEvent OnEnd;


    [SerializeField]
    private OnNextDialogueContext[] DialogueEvents;

    [Inject]
    private InputService inputService;

    [Inject]
    private DialogueManager _dialogueManager;

    public override void HandleInteract(InteractionContext context)
    {
        if (!CanInteract) return;

        CanInteract = false;

        if (!_dialogueManager.TryGetDialog(_dialogId, out var log))
        {
            Debug.LogError($"ShowDialoguePopupInteractableionHandler: Dialog with ID '{_dialogId}' not found.");
            return;
        }

        var view = _dialogueManager.OpenDialog(log);

        view.OnClosed += () =>
        {
            OnEnd.Invoke();

            inputService.EnableMap("Player");
        };

        OnStart.Invoke();

        inputService.DisableMap("Player");

        view.ViewModel.OnNextDialog += (id) => RiseEvent(id);
    }

    private void RiseEvent(int id)
    {
        foreach (var dialogue in DialogueEvents)
        {
            if (id == dialogue.DialogueId) dialogue.On.Invoke();
        }
    }
}