using CommunityToolkit.Mvvm.ComponentModel;

public partial class DialoguePopupViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string title = "[Project]";

    [ObservableProperty]
    private string content = "[Content]";
}