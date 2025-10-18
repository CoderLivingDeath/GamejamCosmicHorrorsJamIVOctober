using System;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class DialogueTrigger : MonoBehaviour
{
    [Serializable]
    private struct OnNextDialogueContext
    {
        public int DialogueId;
        public UnityEvent On;
    }

    [SerializeField]
    private bool IsTriggered = false;

    [SerializeField]
    private bool DestroyAfterExecute;

    [SerializeField]
    private string DialogueId;

    [SerializeField]
    private UnityEvent OnStart;

    [SerializeField]
    private UnityEvent OnEnd;

    [SerializeField]
    private OnNextDialogueContext[] DialogueEvents;

    [Inject]
    private DialogueManager dialogueManager;

    [Inject]
    private InputService inputService;

    void OnTriggerEnter(Collider other)
    {
        if (!IsTriggered)
        {
            var dialogLog = dialogueManager.GetDialog(DialogueId);
            var view = dialogueManager.OpenDialog(dialogLog);

            view.OnClosed += () =>
            {
                OnEnd.Invoke();

                inputService.EnableMap("Player");

                if (DestroyAfterExecute) GameObject.Destroy(this.gameObject);
            };

            OnStart.Invoke();

            inputService.DisableMap("Player");

            view.ViewModel.OnNextDialog += (id) => RiseEvent(id);

            IsTriggered = true;
        }
    }

    private void RiseEvent(int id)
    {
        foreach (var dialogue in DialogueEvents)
        {
            if (id == dialogue.DialogueId) dialogue.On.Invoke();
        }
    }
}