using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

public partial class DoorPuzzlePopupViewModel : ObservableRecipient
{
    private readonly List<int> correctSequence = new List<int> { 1, 3, 2 };
    private int currentIndex = 0;

    public event EventHandler PuzzleSolved;
    public event EventHandler PuzzleFailed;

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
