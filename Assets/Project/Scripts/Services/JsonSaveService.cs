using System;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

public interface IJsonSaveService
{
    T Load<T>(string fileName) where T : class;
    UniTask<T> LoadAsync<T>(string fileName) where T : class;
    void Save<T>(T data, string fileName);
    UniTask SaveAsync<T>(T data, string fileName);
}

public class JsonSaveService : IJsonSaveService
{
    public readonly string DirectoryPath;

    private readonly JsonSerializerSettings jsonSettings;

    // Добавлен параметр с дефолтом для обратной совместимости
    public JsonSaveService(string directoryPath = null)
    {
        this.DirectoryPath = directoryPath ?? Application.streamingAssetsPath;

        if (!Directory.Exists(this.DirectoryPath))
        {
            Directory.CreateDirectory(this.DirectoryPath);
        }

        // Настройки сериализации с конвертерами для Vector2 и Vector3
        jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            // Игнорируем циклические ссылки, если есть
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        // Добавляем пользовательские конвертеры для Vector2 и Vector3
        jsonSettings.Converters.Add(new Vector2Converter());
        jsonSettings.Converters.Add(new Vector3Converter());
    }

    public async UniTask SaveAsync<T>(T data, string fileName)
    {
        string filePath = Path.Combine(DirectoryPath, fileName);

        try
        {
            string json = JsonConvert.SerializeObject(data, jsonSettings);
            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                await writer.WriteAsync(json);
            }
        }
        catch (Exception e)
        {
            throw new IOException($"Ошибка сохранения файла {filePath}: {e.Message}", e);
        }
    }

    public async UniTask<T> LoadAsync<T>(string fileName) where T : class
    {
        string filePath = Path.Combine(DirectoryPath, fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = await reader.ReadToEndAsync();
                try
                {
                    T data = JsonConvert.DeserializeObject<T>(json, jsonSettings);
                    return data;
                }
                catch
                {
                    return null;
                }
            }
        }
        catch (Exception e)
        {
            throw new IOException($"Ошибка загрузки файла {filePath}: {e.Message}", e);
        }
    }

    public async UniTask<bool> UpdatePartialAsync<T>(string fileName, Action<T> updateAction) where T : class, new()
    {
        T data = await LoadAsync<T>(fileName);

        if (data == null)
        {
            data = new T();
        }

        try
        {
            updateAction(data);
            await SaveAsync(data, fileName);
            return true;
        }
        catch (Exception e)
        {
            throw new IOException($"Ошибка частичного обновления файла {fileName}: {e.Message}", e);
        }
    }

    // Синхронный метод сохранения
    public void Save<T>(T data, string fileName)
    {
        string filePath = Path.Combine(DirectoryPath, fileName);

        try
        {
            string json = JsonConvert.SerializeObject(data, jsonSettings);
            File.WriteAllText(filePath, json);
        }
        catch (Exception e)
        {
            throw new IOException($"Ошибка сохранения файла {filePath}: {e.Message}", e);
        }
    }

    // Синхронный метод загрузки
    public T Load<T>(string fileName) where T : class
    {
        string filePath = Path.Combine(DirectoryPath, fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            try
            {
                T data = JsonConvert.DeserializeObject<T>(json, jsonSettings);
                return data;
            }
            catch
            {
                return null;
            }
        }
        catch (Exception e)
        {
            throw new IOException($"Ошибка загрузки файла {filePath}: {e.Message}", e);
        }
    }

    // Синхронный метод частичного обновления
    public bool UpdatePartial<T>(string fileName, Action<T> updateAction) where T : class, new()
    {
        T data = Load<T>(fileName);

        if (data == null)
        {
            data = new T();
        }

        try
        {
            updateAction(data);
            Save(data, fileName);
            return true;
        }
        catch (Exception e)
        {
            throw new IOException($"Ошибка частичного обновления файла {fileName}: {e.Message}", e);
        }
    }
}

public class Vector2Converter : JsonConverter<UnityEngine.Vector2>
{
    public override void WriteJson(JsonWriter writer, UnityEngine.Vector2 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WriteEndObject();
    }

    public override UnityEngine.Vector2 ReadJson(JsonReader reader, Type objectType, UnityEngine.Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        float x = 0f, y = 0f;
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var property = reader.Value.ToString();
                if (!reader.Read()) break;
                switch (property)
                {
                    case "x": x = Convert.ToSingle(reader.Value); break;
                    case "y": y = Convert.ToSingle(reader.Value); break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
                break;
        }
        return new UnityEngine.Vector2(x, y);
    }
}

public class Vector3Converter : JsonConverter<UnityEngine.Vector3>
{
    public override void WriteJson(JsonWriter writer, UnityEngine.Vector3 value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        writer.WritePropertyName("x");
        writer.WriteValue(value.x);
        writer.WritePropertyName("y");
        writer.WriteValue(value.y);
        writer.WritePropertyName("z");
        writer.WriteValue(value.z);
        writer.WriteEndObject();
    }

    public override UnityEngine.Vector3 ReadJson(JsonReader reader, Type objectType, UnityEngine.Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        float x = 0f, y = 0f, z = 0f;
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var property = reader.Value.ToString();
                if (!reader.Read()) break;
                switch (property)
                {
                    case "x": x = Convert.ToSingle(reader.Value); break;
                    case "y": y = Convert.ToSingle(reader.Value); break;
                    case "z": z = Convert.ToSingle(reader.Value); break;
                }
            }
            else if (reader.TokenType == JsonToken.EndObject)
                break;
        }
        return new UnityEngine.Vector3(x, y, z);
    }
}
