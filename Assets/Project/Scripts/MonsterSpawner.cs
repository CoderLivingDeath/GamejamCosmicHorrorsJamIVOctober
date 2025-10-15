using ModestTree;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class MonsterSpawner : MonoBehaviour
{
    [SerializeField]
    private bool IsActivated = false;

    [SerializeField]
    private GameObject Prefab;

    [SerializeField]
    private Transform SpawnPoint;

    [Inject]
    DiContainer diContainer;

    private void OnTriggerEnter(Collider other)
    {
        if (IsActivated)
            return;

        IsActivated = true;

        Debug.Log("WIWIWI");

        // Создаём объект с позицией и ротацией SpawnPoint
        var obj = diContainer.InstantiatePrefab(Prefab, SpawnPoint.position, SpawnPoint.rotation, null);

        obj.GetComponent<MonoCharacterController>().MoveToDirection(new Vector3(-1, 0, 0));
    }
}