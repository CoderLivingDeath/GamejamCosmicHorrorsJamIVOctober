using UnityEngine;
using Zenject;

public class AudioServiceFactory : IFactory<AudioService>
{
    private readonly DiContainer _container;

    public AudioServiceFactory(DiContainer container)
    {
        _container = container;
    }

    AudioService IFactory<AudioService>.Create()
    {
        Camera camera = _container.Resolve<Camera>();
        AudioConfigSO config = _container.Resolve<AudioConfigSO>();

        MonoAudioSourcePool.Factory factory = _container.Resolve<MonoAudioSourcePool.Factory>();

        MonoAudioSourcePool.Factory.CreateConfiguration MusicCreateConfiguration = new("MusicAudioSourcePool", camera.transform, config.MusicPoolSize);
        MonoAudioSourcePool MonoMusicPool = factory.Create(MusicCreateConfiguration);

        MonoAudioSourcePool.Factory.CreateConfiguration SoundCreateConfiguration = new("SoundAudioSourcePool", camera.transform, config.SoundPoolSize);
        MonoAudioSourcePool MonoSoundPool = factory.Create(SoundCreateConfiguration);

        AudioSourcePool musicPool = new AudioSourcePool(MonoMusicPool.Sources, config.MusicGroup);
        AudioSourcePool soundPool = new AudioSourcePool(MonoSoundPool.Sources, config.SoundGroup);

        AudioService service = new(musicPool, soundPool, config);
        return service;
    }
}