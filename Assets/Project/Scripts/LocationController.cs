using System.Collections.Generic;
using UnityEngine;

public class LocationController
{
    private Dictionary<string, GameObject> _locations;

    public LocationController(Dictionary<string, GameObject> locations)
    {
        _locations = locations;
    }

    public void HideLocation(string key)
    {
        if (_locations.TryGetValue(key, out var location))
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
        if (_locations.TryGetValue(key, out var location))
        {
            location.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Location with key '{key}' not found");
        }
    }

    public void ShowOnlyOneLocation(string key)
    {
        foreach (var kv in _locations)
        {
            kv.Value.SetActive(kv.Key == key);
        }
    }
}
