using EditorAttributes;
using GameJamLvl5.Project.Infrastructure.EventBus.Subscribers;
using UnityEngine;
using Zenject;

public class PlayerBehaviour : MonoCharacter, IGameplay_MovementEventHandler, IGameplay_InteractEventHandler
{
    [SerializeField]
    private MonoCharacterController CharacterController;

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
    }

    void OnEnable()
    {
        _eventBus.Subscribe(this);
    }

    void OnDisable()
    {
        _eventBus.Unsubscribe(this);
    }

    #endregion
}