using DG.Tweening;

public static class Animations
{
    public static FadeAnimation InFade(MonoCanvasView view)
    {
        return new FadeAnimation(view)
        .WithDuration(1)
        .WithStartAlpha(0)
        .WithEndAlpha(1)
        .WithEase(Ease.Linear);
    }

    public static FadeAnimation OutFade(MonoCanvasView view)
    {
        return new FadeAnimation(view)
        .WithDuration(1)
        .WithStartAlpha(1)
        .WithEndAlpha(0)
        .WithEase(Ease.Linear);
    }

    public static ShakeAnimation Shake(MonoCanvasView view)
    {
        return new ShakeAnimation(view)
        .WithDuration(1)
        .WithRandomness(1)
        .WithVibrato(1)
        .WithStrength(1)
        .WithEase(Ease.Linear);
    }
}