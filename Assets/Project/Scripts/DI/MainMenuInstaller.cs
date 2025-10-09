using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MainMenuInstaller", menuName = "Installers/MainMenuInstaller")]
public class MainMenuInstaller : ScriptableObjectInstaller<MainMenuInstaller>
{
    public GameObject SettingMenuPrefab;
    public override void InstallBindings()
    {
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();

        Container.Bind<Canvas>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ViewManager>().AsSingle().NonLazy();

        Container.Bind<MainMenuViewModel>().AsTransient();
        Container.Bind<SettingsMenuViewModel>().AsTransient();

        Container.BindFactory<SettingsMenuView, SettingsMenuView.Factory>()
            .FromComponentInNewPrefab(SettingMenuPrefab)
            .AsTransient();

        Container.BindFactory<MonoAudioSourcePool.Factory.CreateConfiguration, MonoAudioSourcePool, MonoAudioSourcePool.Factory>();
        Container.BindInterfacesAndSelfTo<AudioService>().FromFactory<AudioService, AudioServiceFactory>().AsCached().NonLazy();
    }
}
