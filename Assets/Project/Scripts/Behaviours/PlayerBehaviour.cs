using EditorAttributes;
using GameJam.Project.Infrastructure.EventBus.Subscribers;
using GameJamLvl5.Project.Infrastructure.EventBus.Subscribers;
using UnityEngine;
using Zenject;

public class PlayerBehaviour : MonoCharacter, IGameplay_MovementEventHandler, IGameplay_InteractEventHandler, IGameplay_SprintEventHandler
{
    [SerializeField]
    private MonoCharacterController monoCharacterController;

    [SerializeField]
    private GameObject CanInteractMarkObject;

    [Inject]
    private EventBus _eventBus;

    public MonoCharacterController MonoCharacterController => monoCharacterController;

    public void HandleMovement(Vector2 direction)
    {
        monoCharacterController.MoveToDirection(direction);
    }

    public void HandleInteract(bool button)
    {
        monoCharacterController.Interact();
    }

    public void DoDead()
    {
        GameObject.Destroy(this.gameObject);
    }

    #region Unity internal

    [Button]
    public void Snap()
    {
        this.SnapToSurface();
    }

    void Start()
    {
        MonoCharacterController.InteractionController.SelectedInteractableChanged += (item) =>
        {
            CanInteractMarkObject.SetActive(item != null);
        };

    }
    // void Awake()
    // {
    //     CharacterController.InteractionController.SelectedInteractableChanged += (item) =>
    //     {
    //         if (item != null)
    //         {
    //             CanInteractMarkObject.SetActive(true);
    //         }
    //         else
    //         {
    //             CanInteractMarkObject.SetActive(false);
    //         }
    //     };
    // }

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
        monoCharacterController.Sprint(button);
    }

    #endregion
}