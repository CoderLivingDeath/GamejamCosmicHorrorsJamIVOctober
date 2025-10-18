using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class LocationController
{
    public Dictionary<string, MonoLocation> Locations { get; private set; }

    public IEnumerable<KeyValuePair<string, MonoLocation>> ActiveLocations => Locations.Where(item => item.Value.gameObject.activeSelf);

    public LocationSettings locationSettings;

    public LocationController(Dictionary<string, MonoLocation> locations, LocationSettings locationSettings)
    {
        Locations = locations;
        this.locationSettings = locationSettings;
    }

    public void HideLocation(string key)
    {
        if (Locations.TryGetValue(key, out var location))
        {
            location.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"Location with key '{key}' not found");
        }
    }

    public void HideLocations(IEnumerable<string> keys)
    {
        if (keys == null)
        {
            Debug.LogWarning("Keys collection is null");
            return;
        }

        foreach (var key in keys)
        {
            if (Locations.TryGetValue(key, out var location))
            {
                location.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"Location with key '{key}' not found");
            }
        }
    }


    public void ShowLocation(string key)
    {
        if (Locations.TryGetValue(key, out var location))
        {
            location.gameObject.SetActive(true);
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
            Debug.Log(kv.Value.Id);
            bool shouldBeActive = kv.Key == key;
            if (kv.Value.gameObject.activeSelf != shouldBeActive)
            {
                kv.Value.gameObject.SetActive(shouldBeActive);
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

        var keysSet = new HashSet<string>(keys);
        bool keyFound = keysSet.Any(k => Locations.ContainsKey(k));

        if (!keyFound)
        {
            Debug.LogWarning($"None of the specified keys found in locations");
            return;
        }

        foreach (var key in keysSet)
        {
            if (Locations.TryGetValue(key, out var location))
            {
                if (!location.gameObject.activeSelf)
                {
                    location.gameObject.SetActive(true);
                    Debug.Log($"SetActive(true) for location '{key}'");
                }
            }
            else
            {
                Debug.LogWarning($"Location with key '{key}' not found");
            }
        }
    }
}
