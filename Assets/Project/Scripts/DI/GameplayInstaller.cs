using System.Linq;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameplayInstaller", menuName = "Installers/GameplayInstaller")]
public class GameplayInstaller : ScriptableObjectInstaller<GameplayInstaller>
{
    public GameObject DialogViewPrefab;
    public override void InstallBindings()
    {
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();

        // Найти все объекты MonoLocation на сцене, включая неактивные, в неотсортированном порядке (быстрее)
        var locations = Object.FindObjectsByType<MonoLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Создать словарь Id -> GameObject
        var locationsDict = locations.ToDictionary(loc => loc.Id, loc => loc.gameObject);

        Container.BindFactory<MonoAudioSourcePool.Factory.CreateConfiguration, MonoAudioSourcePool, MonoAudioSourcePool.Factory>();
        Container.BindInterfacesAndSelfTo<AudioService>().FromFactory<AudioService, AudioServiceFactory>().AsCached().NonLazy();
        Container.BindInterfacesAndSelfTo<DialogueManager>().AsSingle();

        Container.Bind<Canvas>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ViewManager>().AsSingle().NonLazy();

        Container.BindFactory<DialoguePopupView, DialoguePopupView.Factory>()
        .FromComponentInNewPrefab(DialogViewPrefab)
        .AsTransient();
    }
}