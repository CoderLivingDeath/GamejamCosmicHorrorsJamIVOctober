using EditorAttributes;
using GameJamLvl5.Project.Infrastructure.EventBus.Subscribers;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MovementBehaviour), typeof(InteractionBehaviour))]
public class PlayerBehaviour : MonoCharacter, IGameplay_MovementEventHandler, IGameplay_InteractEventHandler
{
    private MovementBehaviour _movementBehaviour;
    private InteractionBehaviour _interactBehaviour;

    [Inject]
    private EventBus _eventBus;

    public void HandleMovement(Vector2 direction)
    {
        _movementBehaviour.Move(direction);
    }

    public void HandleInteract(bool button)
    {
        _interactBehaviour.Interact();
    }

    [Button]
    public void Snap()
    {
        this.SnapToSurface();
    }

    #region Unity internal

    void Awake()
    {
        _movementBehaviour = GetComponent<MovementBehaviour>();
        _interactBehaviour = GetComponent<InteractionBehaviour>();
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