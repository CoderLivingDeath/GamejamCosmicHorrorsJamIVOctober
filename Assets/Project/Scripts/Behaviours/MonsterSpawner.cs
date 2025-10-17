using UnityEngine;
using Zenject;

public class MonsterSpawner
{
    private DiContainer _container;
    private GameObject _monsterPrefab;

    public MonsterSpawner(DiContainer container, GameObject monsterPrefab)
    {
        _container = container;
        _monsterPrefab = monsterPrefab;
    }

    public GameObject Spawn(Vector3 position, Transform parent = null)
    {
        return _container.InstantiatePrefab(_monsterPrefab, position, Quaternion.identity, parent);
    }
}
