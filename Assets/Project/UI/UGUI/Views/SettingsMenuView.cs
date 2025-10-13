using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public partial class SettingsMenuView : MonoCanvasView
{
    public class Factory : PlaceholderFactory<SettingsMenuView> { }

    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text musicVolumeText;
    [SerializeField] private TMP_Text soundVolumeText;
    [SerializeField] private Button applyBtn;
    [SerializeField] private Button cancelBtn;
    [SerializeField] private TMP_Text applyBtnText;
    [SerializeField] private TMP_Text cancelBtnText;

    [SerializeField] private Slider masterSlider;
    [SerializeField] private TMP_InputField masterSlider_value;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_InputField musicSlider_value;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private TMP_InputField soundSlider_value;

    [Inject] private ViewManager _viewManager;

    [Inject] private SettingsMenuViewModel _viewModel;

    [Space]
    [Header("Animations")]
    [SerializeField]
    private SettingsMenuAnimationsSO animations;

    private bool _isUpdatingUI;

    private void Awake()
    {
        applyBtn.onClick.AddListener(() => _viewModel.ApplyCommand.Execute(null));
        cancelBtn.onClick.AddListener(() => _viewModel.CancelCommand.Execute(null));

        cancelBtn.onClick.AddListener(() =>
        {
            var scope = _viewManager.Animate(animations.Close.ProvideAnimation(this));
            scope.Completed += () => this.Close();
        });

        masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
        masterSlider_value.onEndEdit.AddListener(OnMasterInputChanged);

        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        musicSlider_value.onEndEdit.AddListener(OnMusicInputChanged);

        soundSlider.onValueChanged.AddListener(OnSoundSliderChanged);
        soundSlider_value.onEndEdit.AddListener(OnSoundInputChanged);

    }

    void Start()
    {
    }

    private void OnEnable()
    {
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        UpdateUI();
    }

    private void OnDisable()
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
    }

    private void UpdateUI()
    {
        _isUpdatingUI = true;

        applyBtnText.text = _viewModel.Apply_btn_text;
        cancelBtnText.text = _viewModel.Cancle_btn_text;

        masterVolumeText.text = _viewModel.MasterVolume_Text;
        masterSlider.SetValueWithoutNotify(_viewModel.MasterVolume);
        masterSlider_value.SetTextWithoutNotify(_viewModel.MasterVolume.ToString("0"));

        musicVolumeText.text = _viewModel.MusicVolume_Text;
        musicSlider.SetValueWithoutNotify(_viewModel.MusicVolume);
        musicSlider_value.SetTextWithoutNotify(_viewModel.MusicVolume.ToString("0"));

        soundVolumeText.text = _viewModel.SoundVolume_Text;
        soundSlider.SetValueWithoutNotify(_viewModel.SoundVolume);
        soundSlider_value.SetTextWithoutNotify(_viewModel.SoundVolume.ToString("0"));

        _isUpdatingUI = false;
    }

    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.MusicVolume_Text):
                musicVolumeText.text = _viewModel.MusicVolume_Text;
                break;
            case nameof(_viewModel.SoundVolume_Text):
                soundVolumeText.text = _viewModel.SoundVolume_Text;
                break;
            case nameof(_viewModel.Apply_btn_text):
                applyBtnText.text = _viewModel.Apply_btn_text;
                break;
            case nameof(_viewModel.Cancle_btn_text):
                cancelBtnText.text = _viewModel.Cancle_btn_text;
                break;
            case nameof(_viewModel.MusicVolume):
                musicSlider.SetValueWithoutNotify(_viewModel.MusicVolume);
                musicSlider_value.SetTextWithoutNotify(_viewModel.MusicVolume.ToString("0"));
                break;
            case nameof(_viewModel.SoundVolume):
                soundSlider.SetValueWithoutNotify(_viewModel.SoundVolume);
                soundSlider_value.SetTextWithoutNotify(_viewModel.SoundVolume.ToString("0"));
                break;
            case nameof(_viewModel.MasterVolume_Text):
                masterVolumeText.text = _viewModel.MasterVolume_Text;
                break;
            case nameof(_viewModel.MasterVolume):
                masterSlider.SetValueWithoutNotify(_viewModel.MasterVolume);
                masterSlider_value.SetTextWithoutNotify(_viewModel.MasterVolume.ToString("0"));
                break;
        }
    }

    private void OnMasterSliderChanged(float value)
    {
        if (_isUpdatingUI) return;
        _viewModel.MasterVolume = value;
    }

    private void OnMasterInputChanged(string value)
    {
        if (_isUpdatingUI) return;
        if (float.TryParse(value, out var f))
        {
            f = Mathf.Clamp(f, masterSlider.minValue, masterSlider.maxValue);
            _viewModel.MasterVolume = f;
        }
        else
        {
            UpdateUI();
        }
    }

    private void OnMusicSliderChanged(float value)
    {
        if (_isUpdatingUI) return;
        _viewModel.MusicVolume = value;
    }

    private void OnMusicInputChanged(string value)
    {
        if (_isUpdatingUI) return;
        if (float.TryParse(value, out var f))
        {
            f = Mathf.Clamp(f, musicSlider.minValue, musicSlider.maxValue);
            _viewModel.MusicVolume = f;
        }
        else
        {
            UpdateUI();
        }
    }

    private void OnSoundSliderChanged(float value)
    {
        if (_isUpdatingUI) return;
        _viewModel.SoundVolume = value;
    }

    private void OnSoundInputChanged(string value)
    {
        if (_isUpdatingUI) return;
        if (float.TryParse(value, out var f))
        {
            f = Mathf.Clamp(f, soundSlider.minValue, soundSlider.maxValue);
            _viewModel.SoundVolume = f;
        }
        else
        {
            UpdateUI();
        }
    }
}
