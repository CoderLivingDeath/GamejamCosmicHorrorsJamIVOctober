using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UnityEngine;

public partial class MainMenuViewModel : ObservableRecipient
{

    private const int NEXT_SCENE_ID = 1;

    [ObservableProperty]
    private string title = "Project";

    [ObservableProperty]
    private string play_btn_text = "Play";

    [ObservableProperty]
    private string settings_btn_text = "Settings";

    [ObservableProperty]
    private string exit_btn_text = "Exit";

    private ISceneLoaderService loaderService;

    public MainMenuViewModel(ISceneLoaderService loaderService)
    {
        this.loaderService = loaderService;
    }

    [RelayCommand]
    public void Play()
    {
        var scope = loaderService.LoadScene(NEXT_SCENE_ID);
        scope.ActivateScene();
    }

    [RelayCommand]
    public void Settings()
    {

    }

    [RelayCommand]
    public void Exit()
    {
    }
}