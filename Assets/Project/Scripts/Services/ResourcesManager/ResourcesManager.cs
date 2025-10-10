using System;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager
{
    private IResourcesLoader resourcesLoader;
    private Dictionary<string, WeakReference<object>> cache = new Dictionary<string, WeakReference<object>>();

    public ResourcesManager()
    {
        resourcesLoader = new ResourcesLoader();
    }

    public T Load<T>(string path) where T : UnityEngine.Object
    {
        string cacheKey = $"{path}_{typeof(T).FullName}";
        if (cache.TryGetValue(cacheKey, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var cached) && cached is T cachedTyped)
                return cachedTyped;
            else
                cache.Remove(cacheKey);
        }

        T resource = Resources.Load<T>(path);
        if (resource != null)
            cache[cacheKey] = new WeakReference<object>(resource);
        else
            Debug.LogError($"ResourcesManager: Resource of type {typeof(T)} not found at path {path}");

        return resource;
    }

    public T LoadJson<T>(string path) where T : class
    {
        string cacheKey = $"{path}_json_{typeof(T).FullName}";
        if (cache.TryGetValue(cacheKey, out var weakRef))
        {
            if (weakRef.TryGetTarget(out var cached) && cached is T cachedTyped)
                return cachedTyped;
            else
                cache.Remove(cacheKey);
        }

        T obj = resourcesLoader.LoadAsJson<T>(path);
        if (obj != null)
            cache[cacheKey] = new WeakReference<object>(obj);
        else
            Debug.LogError($"ResourcesManager: Failed to load JSON of type {typeof(T)} at path {path}");

        return obj;
    }

    public void ClearCache()
    {
        cache.Clear();
    }
}
