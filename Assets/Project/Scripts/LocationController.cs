using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LocationController
{
    public Dictionary<string, GameObject> Locations { get; private set; }
    
    public IEnumerable<KeyValuePair<string, GameObject>> ActiveLocations => Locations.Where(item => item.Value.activeSelf);

    public LocationController(Dictionary<string, GameObject> locations)
    {
        Locations = locations;
    }

    public void HideLocation(string key)
    {
        if (Locations.TryGetValue(key, out var location))
        {
            location.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Location with key '{key}' not found");
        }
    }

    public void ShowLocation(string key)
    {
        if (Locations.TryGetValue(key, out var location))
        {
            location.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Location with key '{key}' not found");
        }
    }

    public void ActivateSingleLocation(string key)
    {
        if (!Locations.ContainsKey(key))
        {
            Debug.LogWarning($"Location with key '{key}' not found");
            return;
        }

        foreach (var kv in Locations)
        {
            bool shouldBeActive = kv.Key == key;
            if (kv.Value.activeSelf != shouldBeActive)
            {
                kv.Value.SetActive(shouldBeActive);
                Debug.Log($"SetActive({shouldBeActive}) for location '{kv.Key}'");
            }
        }
    }
    public void ActivateRangeLocation(IEnumerable<string> keys)
    {
        if (keys == null)
        {
            Debug.LogWarning("Keys collection is null");
            return;
        }

        // Для удобства создадим HashSet для быстрого поиска
        var keysSet = new HashSet<string>(keys);

        // Проверим, есть ли хотя бы один ключ в _locations
        bool keyFound = keysSet.Any(k => Locations.ContainsKey(k));
        if (!keyFound)
        {
            Debug.LogWarning($"None of the specified keys found in locations");
            return;
        }

        foreach (var kv in Locations)
        {
            bool shouldBeActive = keysSet.Contains(kv.Key);
            if (kv.Value.activeSelf != shouldBeActive)
            {
                kv.Value.SetActive(shouldBeActive);
                Debug.Log($"SetActive({shouldBeActive}) for location '{kv.Key}'");
            }
        }
    }

}
