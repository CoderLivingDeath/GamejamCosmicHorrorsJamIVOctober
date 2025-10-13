
using DG.Tweening;
using UnityEngine;
[CreateAssetMenu(fileName = "ShakeAnimationSO", menuName = "Animations/ShakeAnimationSO")]
public class ShakeAnimationSO : ViewAnimationSO<MonoCanvasView>
{
    public float Duration = 1f;
    public float Strength = 10f;
    public int Vibrato = 10;
    public float Randomness = 0f;
    public bool FadeOut = true;
    public Ease Ease = Ease.Linear;

    public override ViewAnimation<MonoCanvasView> ProvideAnimation(MonoCanvasView view)
    {
        var animation = new ShakeAnimation(view)
            .WithDuration(Duration)
            .WithStrength(Strength)
            .WithVibrato(Vibrato)
            .WithRandomness(Randomness)
            .WithFadeOut(FadeOut)
            .WithEase(Ease);

        return animation;
    }
}
