using UnityEngine;
using TMPro;
using Zenject;

public class DialoguePopupView : MonoCanvasView
{
    [Inject]
    private DialoguePopupViewModel _viewModel;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text contentText;

    private void OnEnable()
    {
        // Подписываемся на изменение свойств ViewModel для обновления UI
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateUI();
    }

    private void OnDisable()
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.Title) || e.PropertyName == nameof(_viewModel.Content))
        {
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (titleText != null)
            titleText.text = _viewModel.Title;

        if (contentText != null)
            contentText.text = _viewModel.Content;
    }
}
