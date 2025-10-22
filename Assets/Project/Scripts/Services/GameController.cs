using Unity.Cinemachine;
using UnityEngine.Timeline;

public class GameController
{
    private readonly SaveableService saveableService;

    private PlayerBehaviour player;

    private CinemachineCamera camera;

    private MonoCheckpoint activeCheckpoint;

    public GameController(SaveableService saveableService, PlayerBehaviour player, CinemachineCamera camera)
    {
        this.saveableService = saveableService;
        this.player = player;
        this.camera = camera;
    }

    public void OnDead()
    {
        ReturnToChecpoint();
    }

    public void ReturnToChecpoint()
    {
        if (activeCheckpoint != null)
        {
            player.MonoCharacterController.Teleport(activeCheckpoint.PlayerSpawnPoint.position);
            saveableService.RestoreAll();
        }
    }

    public void SetCheckpointAndSaveAll(MonoCheckpoint checkpoint)
    {
        if (activeCheckpoint != null) activeCheckpoint.ActiveCheckpoint = false;

        activeCheckpoint = checkpoint;

        saveableService.SaveAll();
    }
}
