using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

public class SaveableTests
{
    private MonoSaveable saveable;
    private GameObject testGameObject;
    private SaveableService saveableService;
    private JsonSaveServiceMock jsonSaveServiceMock;

    [SetUp]
    public void Setup()
    {
        // Создаём тестовый объект и компонент Saveable
        testGameObject = new GameObject("TestSaveableObject");
        saveable = testGameObject.AddComponent<MonoSaveable>();

        // Добавим пример компонента для параметров
        var rb = testGameObject.AddComponent<Rigidbody>();
        saveable.ComponentParameters = new ComponentParameter[]
        {
            new ComponentParameter { TargetComponent = rb, ParameterName = "mass" }
        };

        // Настраиваем мок-сервис сохранения
        jsonSaveServiceMock = new JsonSaveServiceMock();
        saveableService = new SaveableService(jsonSaveServiceMock);

        // Внедряем mock сервис (эмуляция Zenject)
        typeof(MonoSaveable).GetField("jsonSaveService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(saveable, jsonSaveServiceMock);

        // Генерируем GUID для Saveable
        saveable.GenerateGUID();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.DestroyImmediate(testGameObject);
    }

    [Test]
    public void Capture_ReturnsSnapshotsWithCorrectValues()
    {
        var snapshots = saveable.Capture();

        Assert.IsNotNull(snapshots);
        Assert.AreEqual(1, snapshots.Count);

        var snapshot = snapshots[0];
        Assert.AreEqual("mass", snapshot.ParameterName);
        Assert.AreEqual(saveable.ComponentParameters[0].TargetComponent, snapshot.TargetComponent);
        Assert.AreEqual(typeof(Rigidbody), snapshot.TargetComponent.GetType());
        Assert.AreEqual(1f, snapshot.Value); // По умолчанию Rigidbody mass = 1
    }

    [Test]
    public void Restore_SetsCorrectFieldValue()
    {
        var snapshots = saveable.Capture();

        // Заменим mass на 42
        snapshots[0].Value = 42f;

        saveable.Restore(snapshots);

        // Проверяем обновленное значение Rigidbody.mass
        var rb = (Rigidbody)saveable.ComponentParameters[0].TargetComponent;
        Assert.AreEqual(42f, rb.mass);
    }

    [Test]
    public void SaveAndRestore_UsingSaveableService_WorksCorrectly()
    {
        // Сохраним текущее состояние
        saveableService.Save(saveable);

        // Изменим параметр mass на 5 (искажение)
        var rb = (Rigidbody)saveable.ComponentParameters[0].TargetComponent;
        rb.mass = 5f;

        // Восстановим сохранённые данные
        saveableService.Restore(saveable);

        // mass должен вернуться к исходному (1f)
        Assert.AreEqual(1f, rb.mass);
    }

    [Test]
    public void DebugCapture_CallsSaveOnJsonService()
    {
        saveable.DebugCapture();

        Assert.IsTrue(jsonSaveServiceMock.SaveCalled);
        Assert.AreEqual(saveable.Guid.ToString() + ".json", jsonSaveServiceMock.LastFileName);
        Assert.IsNotNull(jsonSaveServiceMock.LastSavedSnapshot);
    }

    [Test]
    public void DebugRestore_CallsLoadAndRestoresParameters()
    {
        // Подготовим сохранённые данные в мок-сервисе
        var snapshots = saveable.Capture();
        var snapshotWrapper = new Snapshot(saveable.Guid, snapshots);
        jsonSaveServiceMock.SetData(snapshotWrapper, saveable.Guid.ToString() + ".json");

        // Изменим mass
        var rb = (Rigidbody)saveable.ComponentParameters[0].TargetComponent;
        rb.mass = 5f;

        saveable.DebugRestore();

        // mass должен восстановиться к 1f
        Assert.AreEqual(1f, rb.mass);
    }

    [Test]
    public void SaveAll_SavesAllSaveablesOnScene()
    {
        // Создаем еще один Saveable на сцене для проверки множественности
        var additionalGO = new GameObject("AdditionalSaveable");
        var additionalSaveable = additionalGO.AddComponent<MonoSaveable>();
        additionalSaveable.GenerateGUID();

        // Вызов SaveAll должен сохранить оба объекта
        saveableService.SaveAll();

        Assert.Contains(saveable.Guid.ToString() + ".json", jsonSaveServiceMock.LastSavedFilenames());
        Assert.Contains(additionalSaveable.Guid.ToString() + ".json", jsonSaveServiceMock.LastSavedFilenames());

        UnityEngine.Object.DestroyImmediate(additionalGO);
    }

    [Test]
    public void RestoreAll_RestoresAllSaveablesOnScene()
    {
        var additionalGO = new GameObject("AdditionalSaveable");
        var additionalSaveable = additionalGO.AddComponent<MonoSaveable>();

        var rb = additionalGO.AddComponent<Rigidbody>();
        additionalSaveable.ComponentParameters = new ComponentParameter[]
        {
    new ComponentParameter { TargetComponent = rb, ParameterName = "mass" }
        };

        additionalSaveable.GenerateGUID();

        // Сохраняем оба объекта
        saveableService.Save(saveable);
        saveableService.Save(additionalSaveable);

        // Изменяем mass
        ((Rigidbody)saveable.ComponentParameters[0].TargetComponent).mass = 5f;
        ((Rigidbody)additionalSaveable.ComponentParameters[0].TargetComponent).mass = 7f;

        // Восстанавливаем
        saveableService.RestoreAll();

        // Проверяем, что mass восстановился
        Assert.AreEqual(1f, ((Rigidbody)saveable.ComponentParameters[0].TargetComponent).mass);
        Assert.AreEqual(1f, ((Rigidbody)additionalSaveable.ComponentParameters[0].TargetComponent).mass);

        UnityEngine.Object.DestroyImmediate(additionalGO);

    }

}

public class JsonSaveServiceMock : IJsonSaveService
{
    public bool SaveCalled = false;
    public string LastFileName;
    public Snapshot LastSavedSnapshot;

    private readonly Dictionary<string, object> storage = new Dictionary<string, object>();

    public void Save<T>(T data, string fileName)
    {
        SaveCalled = true;
        LastFileName = fileName;
        LastSavedSnapshot = data as Snapshot;
        storage[fileName] = data;
    }

    public T Load<T>(string fileName) where T : class
    {
        if (storage.TryGetValue(fileName, out var data))
            return data as T;
        return null;
    }

    // Для установки данных напрямую в мок
    public void SetData(object data, string fileName)
    {
        storage[fileName] = data;
    }

    public UniTask<T> LoadAsync<T>(string fileName) where T : class
    {
        T data = Load<T>(fileName);
        return UniTask.FromResult(data);
    }

    public UniTask SaveAsync<T>(T data, string fileName)
    {
        Save(data, fileName);
        return UniTask.CompletedTask;
    }

    public string[] LastSavedFilenames()
    {
        return storage.Keys.ToArray();
    }
}
