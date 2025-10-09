using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;


public partial class MainMenuView : MonoCanvasView
{
    public class Factory : PlaceholderFactory<MainMenuView> { }

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button playBtn;
    [SerializeField] private Button settingsBtn;
    [SerializeField] private Button exitBtn;
    [SerializeField] private TMP_Text playBtnText;
    [SerializeField] private TMP_Text settingsBtnText;
    [SerializeField] private TMP_Text exitBtnText;

    [Inject]
    private MainMenuViewModel _viewModel;

    [Inject] private SettingsMenuView.Factory _settingsMenuViewFactory;

    [Inject] private ViewManager ViewManager;

    [Space]
    [Header("Animations")]
    public MainViewAnimationsSO Animations;

    private void OpenSettingsView()
    {
        var creationScope = ViewManager.CreateView(_settingsMenuViewFactory);
        creationScope.WithAnimataion(Animations.SettingsMenu_Open.ProvideAnimation(creationScope.View));
    }

    private void Awake()
    {
        _viewModel.IsActive = true;

        BindViewModel();

        // Добавляем обработчики нажатия кнопок, вызывающие команды ViewModel
        playBtn.onClick.AddListener(() => _viewModel.PlayCommand.Execute(null));
        settingsBtn.onClick.AddListener(() => _viewModel.SettingsCommand.Execute(null));
        exitBtn.onClick.AddListener(() => _viewModel.ExitCommand.Execute(null));

        settingsBtn.onClick.AddListener(() => OpenSettingsView());
    }

    private void OnDestroy()
    {
        UnbindViewModel();
    }

    private void BindViewModel()
    {
        _viewModel.PropertyChanged += ViewModelOnPropertyChanged;

        // Инициализируем UI начальными значениями из ViewModel
        titleText.text = _viewModel.Title;
        playBtnText.text = _viewModel.Play_btn_text;
        settingsBtnText.text = _viewModel.Settings_btn_text;
        exitBtnText.text = _viewModel.Exit_btn_text;
    }

    private void UnbindViewModel()
    {
        if (_viewModel != null)
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
    }

    private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_viewModel.Title):
                titleText.text = _viewModel.Title;
                break;
            case nameof(_viewModel.Play_btn_text):
                playBtnText.text = _viewModel.Play_btn_text;
                break;
            case nameof(_viewModel.Settings_btn_text):
                settingsBtnText.text = _viewModel.Settings_btn_text;
                break;
            case nameof(_viewModel.Exit_btn_text):
                exitBtnText.text = _viewModel.Exit_btn_text;
                break;
        }
    }
}
