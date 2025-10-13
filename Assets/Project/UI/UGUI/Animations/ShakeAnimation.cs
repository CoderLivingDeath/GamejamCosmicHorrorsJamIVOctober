using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;

[System.Serializable]
public class ShakeAnimation : ViewAnimation<MonoCanvasView>
{
    public ShakeAnimation(MonoCanvasView view) : base(view)
    {
    }

    public float Strength { get; set; } = 10f;  // сила дрожания
    public int Vibrato { get; set; } = 10;      // частота колебаний
    public float Randomness { get; set; } = 0; // разброс направления
    public bool FadeOut { get; set; } = true;   // плавное затухание

    public Ease Ease { get; set; } = Ease.Linear;

    public override async UniTask Run(CancellationToken token = default)
    {
        if (View == null || View.gameObject == null)
            return;

        var transform = View.RectTransform;

        // при высоком значение Randomness меняет Z координату
        await transform.DOShakePosition(Duration, Strength, Vibrato, Randomness, fadeOut: FadeOut, snapping: true)
            .SetLink(View.gameObject)
            .SetEase(Ease.InOutBounce)
            .WithCancellation(token);

    }

    public ShakeAnimation WithDuration(float duration)
    {
        this.Duration = duration;
        return this;
    }

    public ShakeAnimation WithStrength(float strength)
    {
        this.Strength = strength;
        return this;
    }

    public ShakeAnimation WithVibrato(int vibrato)
    {
        this.Vibrato = vibrato;
        return this;
    }

    public ShakeAnimation WithRandomness(float randomness)
    {
        this.Randomness = randomness;
        return this;
    }

    public ShakeAnimation WithFadeOut(bool fadeOut)
    {
        this.FadeOut = fadeOut;
        return this;
    }

    public ShakeAnimation WithEase(Ease ease)
    {
        this.Ease = ease;
        return this;
    }
}
