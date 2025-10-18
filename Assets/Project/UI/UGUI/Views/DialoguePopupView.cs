using UnityEngine;
using TMPro;
using Zenject;
using System.ComponentModel;

public class DialoguePopupView : MonoCanvasView, IUI_NextDialogueEventHandler
{
    public class Factory : PlaceholderFactory<DialoguePopupView>
    {
        private readonly DiContainer _container;

        public Factory(DiContainer container)
        {
            _container = container;
        }

        public DialoguePopupView Create(DialogLog log)
        {
            // Zenject создаст экземпляр из префаба автоматически
            var dialogueView = base.Create();

            var localizationService = _container.Resolve<LocalizationService>();
            var viewModel = new DialoguePopupViewModel(log, localizationService);

            dialogueView.SetViewModel(viewModel);
            return dialogueView;
        }
    }

    private DialoguePopupViewModel _viewModel;

    [SerializeField]
    private TMP_Text titleText;

    [SerializeField]
    private TMP_Text contentText;

    [Inject]
    private EventBus _eventBus;

    [Inject]
    private DialogueManager _dialogueManager;

    public DialoguePopupViewModel ViewModel => _viewModel;

    private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (_viewModel == null)
            return;

        switch (e.PropertyName)
        {
            case nameof(_viewModel.Title):
                if (titleText != null)
                    titleText.text = _viewModel.Title ?? "";
                break;

            case nameof(_viewModel.Content):
                if (contentText != null)
                    contentText.text = _viewModel.Content ?? "";
                break;
        }
    }

    private void UpdateUI()
    {
        if (_viewModel == null)
            return;

        if (titleText != null)
            titleText.text = _viewModel.Title ?? "";

        if (contentText != null)
            contentText.text = _viewModel.Content ?? "";
    }

    // Метод для присвоения ViewModel и подписки
    public void SetViewModel(DialoguePopupViewModel viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = viewModel;

        if (_viewModel != null && isActiveAndEnabled)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            UpdateUI();
        }
    }

    // Дополнительный публичный метод для обновления UI из вне
    public void Refresh()
    {
        UpdateUI();
    }

    public void HandleNextDialogueEvent()
    {
        if (!_viewModel.HasNext())
        {
            _dialogueManager.CloseDialog();
        }
        _viewModel.NextMessage();
    }

    protected override void OnFocus()
    {
        base.Defocus();

        _eventBus.Subscribe(this);
    }

    protected override void OnDefocus()
    {
        base.OnDefocus();

        _eventBus.Unsubscribe(this);
    }

    #region Unity Internal

    void OnDestroy()
    {
        _eventBus.Unsubscribe(this);
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    #endregion
}
