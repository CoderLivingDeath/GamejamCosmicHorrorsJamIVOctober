using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;
using Zenject.SpaceFighter;

[CreateAssetMenu(fileName = "GameplayInstaller", menuName = "Installers/GameplayInstaller")]
public class GameplayInstaller : ScriptableObjectInstaller<GameplayInstaller>
{
    public GameObject DialogViewPrefab;
    public GameObject PuzzlePrefab;
    public GameObject MonsterPrefab;

    public LocationSettings locationSettings;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<JsonSaveService>().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerSaveService>().AsSingle();

        Container.BindInterfacesAndSelfTo<SaveableService>().AsSingle();

        Container.BindInterfacesAndSelfTo<CinemachineCamera>().FromComponentInHierarchy().AsSingle();
        Container.Bind<PlayerBehaviour>().FromComponentInHierarchy().AsSingle();

        Container.BindInterfacesAndSelfTo<GameController>().AsSingle().NonLazy();

        Container.Bind<LocationSettings>().FromInstance(locationSettings).AsSingle();
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();
        BindLocationController();

        Container.BindFactory<MonoAudioSourcePool.Factory.CreateConfiguration, MonoAudioSourcePool, MonoAudioSourcePool.Factory>();
        Container.BindInterfacesAndSelfTo<AudioService>().FromFactory<AudioService, AudioServiceFactory>().AsCached().NonLazy();
        Container.BindInterfacesAndSelfTo<DialogueManager>().AsSingle();

        Container.Bind<Canvas>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ViewManager>().AsSingle().NonLazy();

        Container.BindFactory<DialoguePopupView, DialoguePopupView.Factory>()
        .FromComponentInNewPrefab(DialogViewPrefab)
        .AsSingle();

        Container.BindFactory<DoorPuzzlePopupView, DoorPuzzlePopupView.Factory>()
        .FromComponentInNewPrefab(PuzzlePrefab)
        .AsSingle();

        Container.BindInterfacesAndSelfTo<MonsterSpawner>().AsSingle().WithArguments(MonsterPrefab).NonLazy();
    }

    private void BindLocationController()
    {
        var locations = Object.FindObjectsByType<MonoLocation>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // Проверка на дубликаты Id
        var duplicateIds = locations.GroupBy(loc => loc.Id)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();

        if (duplicateIds.Count > 0)
        {
            Debug.LogError("Duplicate Location Ids found: " + string.Join(", ", duplicateIds));
            // Возможно лучше выбрасывать исключение
            return;
        }

        var locationsDict = locations.ToDictionary(loc => loc.Id, loc => loc);

        Container.BindInterfacesAndSelfTo<LocationController>().AsSingle().WithArguments(locationsDict).NonLazy();
    }

}