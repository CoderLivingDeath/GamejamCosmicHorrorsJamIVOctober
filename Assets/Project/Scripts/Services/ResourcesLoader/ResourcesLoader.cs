using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesLoader : IResourcesLoader
{
    public ResourcesLoader()
    {
    }

    public object Load(string path)
    {
        var resource = Resources.Load(path);
        if (resource != null)
        {
            return resource;
        }

        Debug.LogError($"ResourcesLoader: Resource not found at path {path}");
        return null;
    }

    public T LoadAsJson<T>(string path) where T : class
    {
        var resource = Load(path) as TextAsset;
        if (resource == null)
        {
            Debug.LogError($"ResourcesLoader: Failed to load TextAsset at path {path}");
            return null;
        }
        try
        {
            T obj = JsonConvert.DeserializeObject<T>(resource.text);
            return obj;
        }
        catch (Exception e)
        {
            Debug.LogError($"ResourcesLoader: Failed to deserialize JSON at path {path}. Exception: {e}");
            return null;
        }
    }

}