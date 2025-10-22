using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class JsonSaveServiceTests
{

    [Serializable]
    private class TestData
    {
        public int Number;
        public string Text;
    }
    
    private JsonSaveService saveService;
    private string testDir;
    private string testFileName = "test_save.json";

    [SetUp]
    public void SetUp()
    {
        string testDir = Path.Combine(Application.temporaryCachePath, "YourTestFolder", Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);
        saveService = new JsonSaveService(testDir);
    }

    [TearDown]
    public void TearDown()
    {
        // Удаляем тестовую папку и файлы после теста
        if (Directory.Exists(testDir))
            Directory.Delete(testDir, true);
    }

    [Test]
    public void SaveAndLoad_Synchronous_Works()
    {
        var testData = new TestData { Number = 123, Text = "Sync Test" };

        // Сохраним синхронно (нужно предусмотреть в вашем сервисе такие sync методы)
        saveService.Save(testData, testFileName);

        // Загрузим синхронно
        var loaded = saveService.Load<TestData>(testFileName);

        Assert.IsNotNull(loaded);
        Assert.AreEqual(testData.Number, loaded.Number);
        Assert.AreEqual(testData.Text, loaded.Text);
    }

    [Test]
    public async Task SaveAndLoadAsync_WorksCorrectly()
    {
        var testData = new PlayerData();
        testData.KeyCard["card1"] = true;

        await saveService.SaveAsync(testData, testFileName);
        var loadedData = await saveService.LoadAsync<PlayerData>(testFileName);

        Assert.IsNotNull(loadedData);
        Assert.IsTrue(loadedData.KeyCard.ContainsKey("card1"));
        Assert.IsTrue(loadedData.KeyCard["card1"]);
    }

    [Test]
    public async Task SaveAndLoadEmptyData_WorksCorrectly()
    {
        var testData = new PlayerData(); // KeyCard пустой

        await saveService.SaveAsync(testData, testFileName);
        var loadedData = await saveService.LoadAsync<PlayerData>(testFileName);

        Assert.IsNotNull(loadedData);
        Assert.IsNotNull(loadedData.KeyCard);
        Assert.IsEmpty(loadedData.KeyCard);
    }

    [Test]
    public async Task UpdatePartialAsync_WithValidUpdate_UpdatesData()
    {
        bool updateCalled = false;
        bool result = await saveService.UpdatePartialAsync<PlayerData>(testFileName, data =>
        {
            data.KeyCard["card2"] = true;
            updateCalled = true;
        });

        Assert.IsTrue(result);
        Assert.IsTrue(updateCalled);

        var loadedData = await saveService.LoadAsync<PlayerData>(testFileName);
        Assert.IsNotNull(loadedData);
        Assert.IsTrue(loadedData.KeyCard.ContainsKey("card2"));
        Assert.IsTrue(loadedData.KeyCard["card2"]);
    }

    [Test]
    public async Task UpdatePartialAsync_WhenFileMissing_CreatesNewData()
    {
        string missingFile = "non_existing_partial.json";

        bool result = await saveService.UpdatePartialAsync<PlayerData>(missingFile, data =>
        {
            data.KeyCard["newkey"] = true;
        });

        Assert.IsTrue(result);

        var loadedData = await saveService.LoadAsync<PlayerData>(missingFile);
        Assert.IsNotNull(loadedData);
        Assert.IsTrue(loadedData.KeyCard.ContainsKey("newkey"));
    }

    [Test]
    public async Task UpdatePartialAsync_WithEmptyAction_DoesNotThrow()
    {
        bool result = await saveService.UpdatePartialAsync<PlayerData>(testFileName, data => { });

        Assert.IsTrue(result);

        var loadedData = await saveService.LoadAsync<PlayerData>(testFileName);
        Assert.IsNotNull(loadedData);
    }

    [Test]
    public async Task LoadAsync_NonExistingFile_ReturnsNull()
    {
        var loadedData = await saveService.LoadAsync<PlayerData>("non_existent.json");
        Assert.IsNull(loadedData);
    }

    [Test]
    public async Task LoadAsync_InvalidJson_ReturnsNull()
    {
        string filePath = Path.Combine(saveService.DirectoryPath, testFileName);
        File.WriteAllText(filePath, "invalid json");

        var loadedData = await saveService.LoadAsync<PlayerData>(testFileName);
        Assert.IsNull(loadedData);
    }

    [Test]
    public async Task SaveAsync_WritesFileWithContent()
    {
        var testData = new PlayerData();
        testData.KeyCard["key1"] = true;

        await saveService.SaveAsync(testData, testFileName);

        string path = Path.Combine(saveService.DirectoryPath, testFileName);
        Assert.IsTrue(File.Exists(path), "Файл не создан по ожидаемому пути.");

        string content = File.ReadAllText(path);
        Assert.IsNotEmpty(content);
        Assert.IsTrue(content.Contains("key1"));
    }

}

public class PlayerSaveServiceTests
{
    private PlayerSaveService playerSaveService;

    private JsonSaveService saveService;
    private string testDir;


    [SetUp]
    public void SetUp()
    {
        testDir = Path.Combine(Application.temporaryCachePath, "JsonSaveServiceTests", System.Guid.NewGuid().ToString());
        Directory.CreateDirectory(testDir);

        saveService = new(testDir);
        playerSaveService = new PlayerSaveService(saveService);
    }


    [TearDown]
    public void TearDown()
    {
        // Удаляем тестовую папку и файлы после теста
        if (Directory.Exists(testDir))
            Directory.Delete(testDir, true);
    }

    [Test]
    public async Task SaveAndLoadPlayerData_Works()
    {
        var playerData = new PlayerData();
        playerData.KeyCard["key1"] = true;

        await playerSaveService.SaveAsync(playerData);
        var loaded = await playerSaveService.LoadAsync();

        Assert.IsNotNull(loaded);
        Assert.IsTrue(loaded.KeyCard.ContainsKey("key1"));
        Assert.IsTrue(loaded.KeyCard["key1"]);
    }

    [Test]
    public async Task LoadPlayerData_WhenNoFile_ReturnsNewInstance()
    {
        var loaded = await playerSaveService.LoadAsync();
        Assert.IsNotNull(loaded);
        Assert.IsInstanceOf<PlayerData>(loaded);
    }
}
