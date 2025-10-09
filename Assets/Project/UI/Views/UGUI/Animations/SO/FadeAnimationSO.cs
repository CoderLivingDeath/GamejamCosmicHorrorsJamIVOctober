using DG.Tweening;
using System;
using UnityEngine;
[Serializable]
[CreateAssetMenu(fileName = "FadeAnimationSO", menuName = "Animations/FadeAnimationSO")]
public class FadeAnimationSO : ViewAnimationSO<MonoCanvasView>
{
    public float Duration = 1;
    public float StartAlpha = 0f;
    public float EndAlpha = 1f;
    public Ease Ease = Ease.Linear;

    public override ViewAnimation<MonoCanvasView> ProvideAnimation(MonoCanvasView view)
    {
        return new FadeAnimation(view)
.WithDuration(Duration)
.WithStartAlpha(StartAlpha)
.WithEndAlpha(EndAlpha)
.WithEase(Ease);
    }
}
