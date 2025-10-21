using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveableService
{
    private readonly IJsonSaveService jsonSaveService;

    public SaveableService(IJsonSaveService jsonSaveService)
    {
        this.jsonSaveService = jsonSaveService;
    }

    public void Save(ISaveable saveable)
    {
        if (saveable == null)
            throw new ArgumentNullException(nameof(saveable));

        Guid id = saveable.Guid;
        if (id == Guid.Empty)
            throw new ArgumentException("GUID объекта не может быть пустым.");

        var snapshots = saveable.Capture();
        var snapshotWrapper = new Snapshot(id, snapshots);

        jsonSaveService.Save(snapshotWrapper, id.ToString() + ".json");
    }

    public void Restore(ISaveable saveable)
    {
        if (saveable == null)
            throw new ArgumentNullException(nameof(saveable));

        Guid id = saveable.Guid;
        if (id == Guid.Empty)
        {
            Debug.LogWarning("Restore: Объект имеет пустой GUID. Восстановление невозможно.");
            return;
        }

        string filename = id.ToString() + ".json";
        var snapshotWrapper = jsonSaveService.Load<Snapshot>(filename);
        if (snapshotWrapper == null)
        {
            Debug.LogWarning($"Restore: Данные не найдены в файле '{filename}'.");
            return;
        }

        if (snapshotWrapper.componentParameterSnapshots.TryGetValue(id, out var snapshots))
        {
            saveable.Restore(snapshots);
        }
        else
        {
            Debug.LogWarning($"Restore: Нет снимков параметров для GUID {id}");
        }
    }

    public void SaveAll()
    {
        ISaveable[] saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        .OfType<ISaveable>()
        .ToArray();

        foreach (var saveable in saveables)
        {
            try
            {
                Save(saveable);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveAll: Ошибка при сохранении объекта с GUID {saveable.Guid}: {ex.Message}");
            }
        }

        Debug.Log($"SaveAll: Сохранено объектов: {saveables.Length}");
    }

    public void RestoreAll()
    {
        ISaveable[] saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
        .OfType<ISaveable>()
        .ToArray();

        foreach (var saveable in saveables)
        {
            try
            {
                Restore(saveable);
            }
            catch (Exception ex)
            {
                Debug.LogError($"RestoreAll: Ошибка при восстановлении объекта с GUID {saveable.Guid}: {ex.Message}");
            }
        }

        Debug.Log($"RestoreAll: Восстановлено объектов: {saveables.Length}");
    }
}
