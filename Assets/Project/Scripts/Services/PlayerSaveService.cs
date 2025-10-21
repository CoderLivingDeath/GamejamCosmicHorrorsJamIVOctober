using Cysharp.Threading.Tasks;

public class PlayerSaveService
{
    private readonly JsonSaveService _jsonSaveService;
    private const string SaveFileName = "player_save.json";

    public PlayerSaveService(JsonSaveService jsonSaveService)
    {
        _jsonSaveService = jsonSaveService;
    }

    public async UniTask SaveAsync(PlayerData playerData)
    {
        await _jsonSaveService.SaveAsync(playerData, SaveFileName);
    }

    public async UniTask<PlayerData> LoadAsync()
    {
        var playerData = await _jsonSaveService.LoadAsync<PlayerData>(SaveFileName);
        return playerData ?? new PlayerData();
    }
}
