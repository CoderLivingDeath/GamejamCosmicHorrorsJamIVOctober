using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Zenject;

public class DoorPuzzlePopupView : MonoCanvasView
{
    public class Factory : PlaceholderFactory<DoorPuzzlePopupView>
    {
        private readonly DiContainer _container;

        public Factory(DiContainer container)
        {
            _container = container;
        }

        public override DoorPuzzlePopupView Create()
        {
            var view = base.Create();

            var vm = new DoorPuzzlePopupViewModel();

            view.SetViewModel(vm);
            return view;
        }
    }

    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;

    private List<Button> buttons;

    public DoorPuzzlePopupViewModel ViewModel { get; private set; }

    private readonly Color correctColor = Color.green;
    private readonly Color defaultColor = Color.white;

    public void SetViewModel(DoorPuzzlePopupViewModel viewModel)
    {
        ViewModel = viewModel;

        ViewModel.PuzzleSolved += OnPuzzleSolved;
        ViewModel.PuzzleFailed += OnPuzzleFailed;
    }

    private void Awake()
    {
        buttons = new List<Button> { button1, button2, button3 };

        button1.onClick.AddListener(() => OnButtonClick(1));
        button2.onClick.AddListener(() => OnButtonClick(2));
        button3.onClick.AddListener(() => OnButtonClick(3));
    }

    private void OnButtonClick(int buttonNumber)
    {
        bool correct = ViewModel.OnButtonClicked(buttonNumber);
        if (correct)
        {
            buttons[buttonNumber - 1].image.color = correctColor;
        }
        else
        {
            ResetButtonColors();
        }
    }

    private void OnPuzzleSolved(object sender, EventArgs e)
    {
        Close();
    }

    private void OnPuzzleFailed(object sender, EventArgs e)
    {
        ResetButtonColors();
    }

    private void ResetButtonColors()
    {
        foreach (var btn in buttons)
            btn.image.color = defaultColor;
    }

    private void OnDestroy()
    {
        if (ViewModel != null)
        {
            ViewModel.PuzzleSolved -= OnPuzzleSolved;
            ViewModel.PuzzleFailed -= OnPuzzleFailed;
        }
    }
}
