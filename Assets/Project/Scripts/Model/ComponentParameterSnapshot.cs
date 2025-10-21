using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ComponentParameterSnapshot
{
    [JsonIgnore]
    public Component TargetComponent;

    // Для идентификации компонента при сохранении
    public string GameObjectPath;  // путь к объекту в сцене
    public string ComponentTypeName;  // имя типа компонента

    public string ParameterName;
    public object Value;

    public ComponentParameterSnapshot()
    {

    }

    public ComponentParameterSnapshot(Component targetComponent, string parameterName, object value)
    {
        TargetComponent = targetComponent;
        ParameterName = parameterName;
        Value = value;

        if (targetComponent != null)
        {
            GameObjectPath = GetGameObjectPath(targetComponent.gameObject);
            ComponentTypeName = targetComponent.GetType().AssemblyQualifiedName;
        }
    }

    // Получение полного пути GameObject в иерархии (например "Root/Player/Camera")
    private string GetGameObjectPath(GameObject go)
    {
        string path = go.name;
        Transform current = go.transform.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        return path;
    }

    public void RestoreTargetComponent()
    {
        GameObject go = FindGameObjectByPath(GameObjectPath);
        if (go == null)
        {
            TargetComponent = null;
            Debug.LogWarning($"RestoreTargetComponent: GameObject по пути '{GameObjectPath}' не найден.");
            return;
        }

        Type compType = Type.GetType(ComponentTypeName);
        if (compType == null)
        {
            TargetComponent = null;
            Debug.LogWarning($"RestoreTargetComponent: Тип компонента '{ComponentTypeName}' не найден.");
            return;
        }

        TargetComponent = go.GetComponent(compType);
        if (TargetComponent == null)
        {
            Debug.LogWarning($"RestoreTargetComponent: Компонент типа '{ComponentTypeName}' не найден на объекте '{GameObjectPath}'.");
        }
    }


    private GameObject FindGameObjectByPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return null;

        string[] parts = path.Split('/');
        GameObject current = null;

        // Найти корневой объект сцены с именем parts[0]
        foreach (var rootObj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (rootObj.name == parts[0])
            {
                current = rootObj;
                break;
            }
        }

        if (current == null)
            return null;

        // Последовательно пройтись по потомкам
        for (int i = 1; i < parts.Length; i++)
        {
            Transform child = current.transform.Find(parts[i]);
            if (child == null)
                return null;
            current = child.gameObject;
        }

        return current;
    }

}
