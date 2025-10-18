using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityEngine;

public partial class DialoguePopupViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string title = "[Project]";

    [ObservableProperty]
    private string content = "[Content]";

    [ObservableProperty]
    public bool hasLastMessage = false;

    private readonly LocalizationService _localizationService;
    private readonly DialogLog _dialogLogModel;
    private int current = -1;

    public event Action<int> OnNextDialog;

    public DialoguePopupViewModel(DialogLog dialogLog, LocalizationService localizationService)
    {
        _dialogLogModel = dialogLog ?? throw new ArgumentNullException(nameof(dialogLog));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        NextMessage();
    }

    [RelayCommand]
    public void NextMessage()
    {
        if (HasLastMessage)
        {
            Debug.LogWarning("This last message.");
            return;
        }

        if (_dialogLogModel?.lines == null || _dialogLogModel.lines.Count == 0)
        {
            Title = "[No Messages]";
            Content = "";
            return;
        }

        current++;

        if (current >= _dialogLogModel.lines.Count)
        {
            HasLastMessage = true;
            return;
        }

        var line = _dialogLogModel.lines[current];
        if (line == null)
        {
            Title = "[Missing Line]";
            Content = "";
            return;
        }

        var newTitle = _localizationService.GetValue(line.titleId) ?? "[Unknown Title]";
        var newContent = _localizationService.GetValue(line.textId) ?? "[Unknown Content]";

        Title = newTitle;
        Content = newContent;

        OnNextDialog?.Invoke(current);
    }

    public void Reset()
    {
        HasLastMessage = false;
        current = -1;
        Title = "[Project]";
        Content = "[Content]";
    }

    public bool HasNext()
    {
        return _dialogLogModel?.lines != null && current + 1 < _dialogLogModel.lines.Count;
    }
}
