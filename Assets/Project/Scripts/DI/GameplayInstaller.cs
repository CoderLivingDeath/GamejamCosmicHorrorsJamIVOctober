using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameplayInstaller", menuName = "Installers/GameplayInstaller")]
public class GameplayInstaller : ScriptableObjectInstaller<GameplayInstaller>
{
    public override void InstallBindings()
    {
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();
        
        Container.BindFactory<MonoAudioSourcePool.Factory.CreateConfiguration, MonoAudioSourcePool, MonoAudioSourcePool.Factory>();
        Container.BindInterfacesAndSelfTo<AudioService>().FromFactory<AudioService, AudioServiceFactory>().AsCached().NonLazy();
    }
}