using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;

[Serializable]
public class FadeAnimation : ViewAnimation<MonoCanvasView>
{
    public FadeAnimation(MonoCanvasView view) : base(view)
    {

    }

    public float StartAlpha { get; set; } = 0f;
    public float EndAlpha { get; set; } = 1f;
    public Ease Ease { get; set; } = Ease.Linear;

    public override async UniTask Run(CancellationToken token = default)
    {
        var canvasGroup = View.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = View.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = StartAlpha;

        await canvasGroup.DOFade(EndAlpha, Duration)
            .SetEase(Ease)
            .SetLink(View.gameObject)
            .WithCancellation(token);

        canvasGroup.alpha = EndAlpha;
    }

    public FadeAnimation WithDuration(float duration)
    {
        this.Duration = duration;
        return this;
    }

    public FadeAnimation WithStartAlpha(float startAlpha)
    {
        this.StartAlpha = startAlpha;
        return this;
    }

    public FadeAnimation WithEndAlpha(float endAlpha)
    {
        this.EndAlpha = endAlpha;
        return this;
    }

    public FadeAnimation WithEase(Ease ease)
    {
        this.Ease = ease;
        return this;
    }
}
