using EditorAttributes;
using GameJam.Project.Infrastructure.EventBus.Subscribers;
using GameJamLvl5.Project.Infrastructure.EventBus.Subscribers;
using UnityEngine;
using Zenject;

public class PlayerBehaviour : MonoCharacter, IGameplay_MovementEventHandler, IGameplay_InteractEventHandler, IGameplay_SprintEventHandler
{
    [SerializeField]
    private MonoCharacterController CharacterController;

    [SerializeField]
    private GameObject CanInteractMarkObject;

    [Inject]
    private EventBus _eventBus;

    public void HandleMovement(Vector2 direction)
    {
        CharacterController.MoveToDirection(direction);
    }

    public void HandleInteract(bool button)
    {
        CharacterController.Interact();
    }

    [Button]
    public void Snap()
    {
        this.SnapToSurface();
    }

    #region Unity internal

    void Awake()
    {
        CharacterController.InteractionController.SelectedInteractableChanged += (item) =>
        {
            if (item != null)
            {
                CanInteractMarkObject.SetActive(true);
            }
            else
            {
                CanInteractMarkObject.SetActive(false);
            }
        };
    }

    void OnEnable()
    {
        _eventBus.Subscribe(this);
    }

    void OnDisable()
    {
        _eventBus.Unsubscribe(this);
    }

    public void HandleSprint(bool button)
    {
        CharacterController.Sprint(button);
    }

    #endregion
}