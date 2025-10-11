using System;
using System.Collections.Generic;
using UnityEngine; // для Debug.LogError, если нужно

public class DialogueManager
{
    private const string PATH = "Localization/Dialogues";
    private readonly ResourcesManager resourcesManager;
    private readonly ViewManager _viewManager;

    private readonly DialoguePopupView.Factory _popupFactory;

    private DialoguePopupView _currentDialogueView;

    private DialogsData cachedDialogsData;

    public DialogueManager(ResourcesManager resourcesManager, ViewManager viewManager, DialoguePopupView.Factory popupFactory)
    {
        this.resourcesManager = resourcesManager ?? throw new ArgumentNullException(nameof(resourcesManager));
        this._viewManager = viewManager;
        _popupFactory = popupFactory;
    }

    /// <summary>
    /// Получить диалог по ключу.
    /// </summary>
    /// <param name="key">Ключ диалога</param>
    /// <returns>Диалог, если найден</returns>
    /// <exception cref="KeyNotFoundException">Если ключ не найден</exception>
    /// <exception cref="InvalidOperationException">Если ресурсы не загружены</exception>
    public DialogLog GetDialog(string key)
    {
        if (cachedDialogsData == null)
        {
            if (!resourcesManager.TryLoadJson<DialogsData>(PATH, out cachedDialogsData) || cachedDialogsData == null)
            {
                var message = $"DialogueManager: Failed to load dialogs from path '{PATH}'";
                Debug.LogError(message);
                throw new InvalidOperationException(message);
            }
        }

        if (cachedDialogsData.Dialogs != null && cachedDialogsData.Dialogs.TryGetValue(key, out var dialogLog))
        {
            return dialogLog;
        }
        else
        {
            var message = $"DialogueManager: Dialog key '{key}' not found in dialogs.";
            Debug.LogError(message);
            throw new KeyNotFoundException(message);
        }
    }

    /// <summary>
    /// Безопасная альтернатива для получения диалога.
    /// </summary>
    public bool TryGetDialog(string key, out DialogLog dialogLog)
    {
        dialogLog = null;

        if (cachedDialogsData == null)
        {
            if (!resourcesManager.TryLoadJson<DialogsData>(PATH, out cachedDialogsData) || cachedDialogsData == null)
            {
                Debug.LogError($"DialogueManager: Failed to load dialogs from path '{PATH}'");
                return false;
            }
        }

        if (cachedDialogsData.Dialogs != null && cachedDialogsData.Dialogs.TryGetValue(key, out dialogLog))
        {
            return true;
        }

        return false;
    }

    public ViewCreationScope<DialoguePopupView> OpenDialog(DialogLog log)
    {
        CloseDialog();

        var scope = _viewManager.CreateView(() => _popupFactory.Create(log));
        _viewManager.SetFocus(scope.View);

        _currentDialogueView = scope.View;

        return scope;
    }

    public void CloseDialog()
    {
        if (_currentDialogueView != null)
        {
            _currentDialogueView.Close();
            _currentDialogueView = null;
        }
    }
}