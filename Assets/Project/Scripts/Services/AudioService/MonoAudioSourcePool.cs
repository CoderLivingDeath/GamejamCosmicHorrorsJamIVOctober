using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class MonoAudioSourcePool : MonoBehaviour
{
    public class Factory : PlaceholderFactory<Factory.CreateConfiguration, MonoAudioSourcePool>
    {
        public readonly struct CreateConfiguration
        {
            public readonly string PoolName;
            public readonly Transform Parent;
            public readonly int PoolSize;

            public CreateConfiguration(string poolName, Transform parent, int poolSize)
            {
                PoolName = poolName;
                Parent = parent;
                PoolSize = poolSize;
            }
        }

        public override MonoAudioSourcePool Create(CreateConfiguration createConfiguration)
        {
            GameObject poolObject = new GameObject(createConfiguration.PoolName);
            if (createConfiguration.Parent != null)
                poolObject.transform.SetParent(createConfiguration.Parent, false);

            MonoAudioSourcePool pool = poolObject.AddComponent<MonoAudioSourcePool>();

            // Создаем дочерние объекты с AudioSource
            for (int i = 0; i < createConfiguration.PoolSize; i++)
            {
                GameObject sourceObject = new GameObject("source_" + i);
                sourceObject.transform.SetParent(poolObject.transform, false);

                AudioSource audioSource = sourceObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;

                pool.Add(audioSource);
            }

            return pool;
        }
    }

    [SerializeField]
    private List<AudioSource> _sources = new List<AudioSource>();

    public IEnumerable<AudioSource> Sources => _sources;

    public void Add(AudioSource source)
    {
        _sources.Add(source);
    }

}