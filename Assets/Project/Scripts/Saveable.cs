using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EditorAttributes;
using UnityEngine;
using Zenject;

public class MonoSaveable : MonoBehaviour, ISaveable
{

    [Inject]
    private IJsonSaveService jsonSaveService;

    public Guid Guid
    {
        get
        {
            if (Guid.TryParse(ID, out var guid))
                return guid;
            else
                return Guid.Empty;
        }
    }

    public string ID;

    public void GenerateGUID()
    {
        ID = Guid.NewGuid().ToString();
    }

    public ComponentParameter[] ComponentParameters;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(ID) || ID == Guid.Empty.ToString())
        {
            GenerateGUID();
            // Помечаем сцену изменённой, чтобы сохранить новый ID
#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
    [ContextMenu("Debug")]
    public void InternalDebug()
    {
        if (ComponentParameters == null || ComponentParameters.Length == 0)
        {
            Debug.Log("No ComponentParameters to debug.");
            return;
        }

        foreach (var param in ComponentParameters)
        {
            if (param == null)
            {
                Debug.LogWarning("Null ComponentParameter found.");
                continue;
            }

            Component component = param.TargetComponent;
            string paramName = param.ParameterName;

            if (component == null)
            {
                Debug.LogWarning("ComponentParameter with null TargetComponent.");
                continue;
            }

            var compType = component.GetType();

            if (string.IsNullOrEmpty(paramName))
            {
                Debug.LogWarning($"ComponentParameter on component '{compType.Name}' has empty ParameterName.");
                continue;
            }

            // Поиск поля с таким именем
            var field = compType.GetField(paramName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (field != null)
            {
                object value = field.GetValue(component);
                Debug.Log($"Component: {compType.Name}, Field: {paramName}, Value: {value}");
                continue;
            }

            // Поиск свойства с таким именем
            var prop = compType.GetProperty(paramName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic);

            if (prop != null && prop.CanRead)
            {
                object value = prop.GetValue(component);
                Debug.Log($"Component: {compType.Name}, Property: {paramName}, Value: {value}");
                continue;
            }

            Debug.LogWarning($"Parameter '{paramName}' not found on component '{compType.Name}'.");
        }
    }

    [ContextMenu("Debug Capture")]
    public void DebugCapture()
    {
        var snapshotparametres = Capture();
        Snapshot snapshot = new(Guid, snapshotparametres);
        jsonSaveService.Save(snapshot, Guid.ToString() + ".json");
    }

    [ContextMenu("Debug Restore")]
    public void DebugRestore()
    {
        var snapshot = jsonSaveService.Load<Snapshot>(Guid.ToString() + ".json");
        if (snapshot == null)
        {
            Debug.LogWarning("Snapshot load returned null.");
            return;
        }

        foreach (var kvp in snapshot.componentParameterSnapshots)
        {
            var guid = kvp.Key;
            var paramList = kvp.Value;

            foreach (var param in paramList)
            {
                param.RestoreTargetComponent();

                if (param.TargetComponent == null)
                {
                    Debug.LogWarning($"Component for '{param.GameObjectPath}' with type '{param.ComponentTypeName}' not found.");
                    continue;
                }

                var compType = param.TargetComponent.GetType();

                var field = compType.GetField(param.ParameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                {
                    try
                    {
                        object converted = ConvertToExpectedType(param.Value, field.FieldType);
                        field.SetValue(param.TargetComponent, converted);
                        Debug.Log($"Field '{param.ParameterName}' of component '{compType.Name}' restored.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to set field '{param.ParameterName}': {ex.Message}");
                    }
                    continue;
                }

                var prop = compType.GetProperty(param.ParameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null && prop.CanWrite)
                {
                    try
                    {
                        object converted = ConvertToExpectedType(param.Value, prop.PropertyType);
                        prop.SetValue(param.TargetComponent, converted);
                        Debug.Log($"Property '{param.ParameterName}' of component '{compType.Name}' restored.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to set property '{param.ParameterName}': {ex.Message}");
                    }
                    continue;
                }

                Debug.LogWarning($"Parameter '{param.ParameterName}' not found on component '{compType.Name}'.");
            }
        }
    }
    private object ConvertToExpectedType(object value, Type targetType)
    {
        if (value == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        try
        {
            // Специальная обработка Unity типов
            if (targetType == typeof(Vector3))
            {
                if (value is Newtonsoft.Json.Linq.JObject jObj)
                    return jObj.ToObject<Vector3>();
            }
            else if (targetType == typeof(Vector2))
            {
                if (value is Newtonsoft.Json.Linq.JObject jObj)
                    return jObj.ToObject<Vector2>();
            }
            else if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, value.ToString());
            }

            return Convert.ChangeType(value, targetType);
        }
        catch
        {
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }

    private object ConvertValueToTargetType(object sourceValue, Type targetType)
    {
        if (sourceValue == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        var sourceType = sourceValue.GetType();

        if (targetType.IsAssignableFrom(sourceType))
            return sourceValue;

        try
        {
            // Попытка обычного преобразования
            return Convert.ChangeType(sourceValue, targetType);
        }
        catch
        {
            // Специальные случаи для Unity Vector и др.
            if (targetType == typeof(Vector2) && sourceValue is Vector3 v3)
                return new Vector2(v3.x, v3.y);
            if (targetType == typeof(Vector3) && sourceValue is Vector2 v2)
                return new Vector3(v2.x, v2.y, 0f);
            if (targetType == typeof(Vector4) && sourceValue is Vector3 v3_2)
                return new Vector4(v3_2.x, v3_2.y, v3_2.z, 0f);

            // Если не удалось преобразовать, возвращаем значение по умолчанию
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
        }
    }


    // TODO: сделать дебаг на это
    public IList<ComponentParameterSnapshot> Capture()
    {
        var snapshots = new List<ComponentParameterSnapshot>();

        if (ComponentParameters == null || ComponentParameters.Length == 0)
            return snapshots;

        foreach (var param in ComponentParameters)
        {
            if (param == null || param.TargetComponent == null || string.IsNullOrEmpty(param.ParameterName))
                continue;

            var comp = param.TargetComponent;
            var type = comp.GetType();

            // Ищем поле
            var field = type.GetField(param.ParameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                var value = field.GetValue(comp);
                snapshots.Add(new ComponentParameterSnapshot(comp, param.ParameterName, value));
                continue;
            }

            // Ищем свойство
            var prop = type.GetProperty(param.ParameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanRead)
            {
                var value = prop.GetValue(comp);
                snapshots.Add(new ComponentParameterSnapshot(comp, param.ParameterName, value));
                continue;
            }

            Debug.LogWarning($"Parameter '{param.ParameterName}' not found in component '{type.Name}'");
        }

        return snapshots;
    }

    // TODO: сделать дебаг на это
    public void Restore(IList<ComponentParameterSnapshot> snapshots)
    {
        if (snapshots == null)
            return;

        foreach (var snapshot in snapshots)
        {
            if (snapshot == null || string.IsNullOrEmpty(snapshot.ParameterName))
                continue;

            // Восстанавливаем корректный TargetComponent по пути и типу
            snapshot.RestoreTargetComponent();

            if (snapshot.TargetComponent == null)
            {
                Debug.LogWarning($"Restore: Component for '{snapshot.GameObjectPath}' with type '{snapshot.ComponentTypeName}' not found.");
                continue;
            }

            var comp = snapshot.TargetComponent;
            var type = comp.GetType();

            var field = type.GetField(snapshot.ParameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                try
                {
                    var value = ConvertToExpectedType(snapshot.Value, field.FieldType);
                    field.SetValue(comp, value);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to set field '{snapshot.ParameterName}' on '{type.Name}': {ex.Message}");
                }
                continue;
            }

            var prop = type.GetProperty(snapshot.ParameterName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    var value = ConvertValueToTargetType(snapshot.Value, prop.PropertyType);
                    prop.SetValue(comp, value);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to set property '{snapshot.ParameterName}' on '{type.Name}': {ex.Message}");
                }
                continue;
            }

            Debug.LogWarning($"Parameter '{snapshot.ParameterName}' not found or not writable in component '{type.Name}'");
        }
    }


}
