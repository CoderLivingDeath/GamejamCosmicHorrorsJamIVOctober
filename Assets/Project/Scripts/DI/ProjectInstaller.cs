using System.Collections.Generic;
using GameJamLvl5.Project.Infrastructure.EventBus.Subscribers;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

[CreateAssetMenu(fileName = "ProjectInstaller", menuName = "Installers/ProjectInstaller")]
public class ProjectInstaller : ScriptableObjectInstaller<ProjectInstaller>
{
    public AudioConfigSO audioConfigSO;
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<AudioConfigSO>().FromInstance(audioConfigSO);
        Container.BindInterfacesAndSelfTo<InputSystem_Actions>().FromMethod((context) => new InputSystem_Actions()).AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<ResourcesLoader>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ResourcesManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<LocalizationService>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<EventBus>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<SceneLoaderService>().AsSingle();
        Container.BindInterfacesAndSelfTo<InputService>().FromFactory<InputService, InputServiceFactory>().AsCached().NonLazy();
    }
}

public class InputServiceFactory : IFactory<InputService>
{
    private DiContainer container;

    public InputServiceFactory(DiContainer container)
    {
        this.container = container;
    }

    public InputService Create()
    {
        var inputAsset = container.Resolve<InputSystem_Actions>().asset;
        var bus = container.Resolve<EventBus>();

        InputService service = new(inputAsset);

        service.Subscribe(new("Player", "Move"),
            (MoveHandler, InputActionType.Performed),
            (MoveHandler, InputActionType.Canceled));

        service.Subscribe(new("Player", "Interact"), InteractHandler, InputActionType.Performed);
        service.Subscribe(new("UI", "Submit"), NextDialogueHandler, InputActionType.Performed);

        return service;

        void MoveHandler(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Vector2>();
            bus.RaiseEvent<IGameplay_MovementEventHandler>(h => h.HandleMovement(value));
        }

        void InteractHandler(InputAction.CallbackContext context)
        {
            var value = context.ReadValueAsButton();
            bus.RaiseEvent<IGameplay_InteractEventHandler>(h => h.HandleInteract(value));
        }

        void NextDialogueHandler(InputAction.CallbackContext context)
        {
            bus.RaiseEvent<IUI_NextDialogueEventHandler>(h => h.HandleNextDialogueEvent());
        }
    }


}