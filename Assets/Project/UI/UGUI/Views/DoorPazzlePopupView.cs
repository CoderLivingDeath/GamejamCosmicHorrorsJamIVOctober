using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

public class DoorPazzlePopupView : MonoCanvasView
{
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;

    private List<Button> buttons;

    public DoorPazzlePopupViewModel ViewModel { get; private set; }

    private readonly Color correctColor = Color.green;
    private readonly Color defaultColor = Color.white;

    private void Awake()
    {
        ViewModel = new DoorPazzlePopupViewModel();

        buttons = new List<Button> { button1, button2, button3 };

        button1.onClick.AddListener(() => OnButtonClick(1));
        button2.onClick.AddListener(() => OnButtonClick(2));
        button3.onClick.AddListener(() => OnButtonClick(3));

        ViewModel.PuzzleSolved += OnPuzzleSolved;
        ViewModel.PuzzleFailed += OnPuzzleFailed; // Новое событие для ошибки
    }

    private void OnButtonClick(int buttonNumber)
    {
        bool correct = ViewModel.OnButtonClicked(buttonNumber);
        if (correct)
        {
            // Меняем цвет на правильный для последней нажатой кнопки
            buttons[buttonNumber - 1].image.color = correctColor;
        }
        else
        {
            // Сброс цвета на все кнопки
            ResetButtonColors();
            
        }
    }

    private void OnPuzzleSolved(object sender, EventArgs e)
    {
        this.Close();
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
        ViewModel.PuzzleSolved -= OnPuzzleSolved;
        ViewModel.PuzzleFailed -= OnPuzzleFailed;
    }
}


public partial class DoorPazzlePopupViewModel : ObservableRecipient
{
    private readonly List<int> correctSequence = new List<int> { 1, 3, 2 };
    private int currentIndex = 0;

    public event EventHandler PuzzleSolved;
    public event EventHandler PuzzleFailed; // Новое событие при ошибке

    // Возвращаем true если правильная кнопка, false если ошибка
    public bool OnButtonClicked(int buttonNumber)
    {
        if (buttonNumber == correctSequence[currentIndex])
        {
            currentIndex++;
            if (currentIndex == correctSequence.Count)
            {
                PuzzleSolved?.Invoke(this, EventArgs.Empty);
                currentIndex = 0;
            }
            return true;
        }
        else
        {
            currentIndex = 0;
            PuzzleFailed?.Invoke(this, EventArgs.Empty);
            return false;
        }
    }
}
