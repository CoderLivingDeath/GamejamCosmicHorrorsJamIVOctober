using UnityEngine;
using Zenject;

public class MonoCheckpoint : MonoBehaviour
{
    public bool ActiveCheckpoint;
    public Transform PlayerSpawnPoint;

    [Inject]
    private GameController gameController;

    public void Activate()
    {
        if (ActiveCheckpoint) return;

        gameController.SetCheckpointAndSaveAll(this);
        ActiveCheckpoint = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Activate();
    }

    [ContextMenu("Debug Return")]
    public void DebugReturnToCheckpointWithRestoreAll()
    {
        gameController.ReturnToChecpoint();
    }
}