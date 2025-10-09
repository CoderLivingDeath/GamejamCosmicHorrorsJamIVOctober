using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class SettingsMenuViewModel : ObservableRecipient
{
    [ObservableProperty] private string masterVolume_Text = "Master Volume";
    [ObservableProperty] private string musicVolume_Text = "Music Volume";
    [ObservableProperty] private string soundVolume_Text = "Sound Volume";
    [ObservableProperty] private string apply_btn_text = "Apply";
    [ObservableProperty] private string cancle_btn_text = "Cancel";

    // �������� (���� ���������). ��������� �������������� ��� ��������!
    [ObservableProperty] private float masterVolume = 80;
    [ObservableProperty] private float musicVolume = 80;
    [ObservableProperty] private float soundVolume = 80;

    private AudioService _audioService;

    partial void OnMasterVolumeChanged(float oldValue, float newValue)
    {
        _audioService.SetMasterVolume(newValue / 100);
    }

    partial void OnSoundVolumeChanged(float oldValue, float newValue)
    {
        _audioService.SetSoundVolume(newValue / 100);
    }

    partial void OnMusicVolumeChanged(float oldValue, float newValue)
    {
        _audioService.SetMusicVolume(newValue / 100);
    }

    public SettingsMenuViewModel(AudioService audioService)
    {
        _audioService = audioService;

        MasterVolume = _audioService.GetMasterVolume() * 100;
        MusicVolume = _audioService.GetMusicVolume() * 100;
        soundVolume = _audioService.GetSoundVolume() * 100;
    }

    [RelayCommand]
    private void Apply()
    {
    }

    [RelayCommand]
    private void Cancel()
    {
    }
}
