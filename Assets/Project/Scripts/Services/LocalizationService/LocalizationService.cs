using System;
using UnityEngine;

public class LocalizationService
{
    public string Localization = "";

    public event Action<string> LocalizationChanged;

    private LocalizationData _localizationData;
    private ResourcesManager _resourcesManager;

    public LocalizationService(ResourcesManager resourcesManager)
    {
        this._resourcesManager = resourcesManager;
        SetLocalization("Ru");
    }

    public void SetLocalization(string newLocalization)
    {
        if (newLocalization == Localization) return;
        
        if(_resourcesManager.TryLoadJson<LocalizationData>($"Localization/{newLocalization}", out var newlocalizationData))
        {
            _localizationData = newlocalizationData;
            Localization = newLocalization;

            LocalizationChanged?.Invoke(Localization);
        }
    }

    public string GetValue(string key)
    {
        if (_localizationData.Keys.TryGetValue(key, out var message))
        {
            return message;
        }
        else
        {
            Debug.LogWarning($"Message key {key} not found.");
            return $"[key:{key}]";
        }
    }
}