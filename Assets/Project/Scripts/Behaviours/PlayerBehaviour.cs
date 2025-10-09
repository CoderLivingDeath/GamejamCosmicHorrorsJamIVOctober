using GameJamLvl5.Project.Infrastructure.EventBus.Subscribers;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MovementBehaviour))]
public class PlayerBehaviour : MonoBehaviour, IGameplay_MovementEventHandler
{

    private MovementBehaviour movementBehaviour;

    [Inject]
    private EventBus _eventBus;

    void Awake()
    {
        movementBehaviour = GetComponent<MovementBehaviour>();
    }

    void OnEnable()
    {
        _eventBus.Subscribe(this);
    }

    void OnDisable()
    {
        _eventBus.Unsubscribe(this);
    }

    public void HandleMovement(Vector2 direction)
    {
        movementBehaviour.Move(direction);
    }
}