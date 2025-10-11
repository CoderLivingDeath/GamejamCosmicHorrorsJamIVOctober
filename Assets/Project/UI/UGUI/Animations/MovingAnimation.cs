using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;

public class MovingAnimation : ViewAnimation<MonoCanvasView>
{
    public MovingAnimation(MonoCanvasView view) : base(view)
    {
    }


    public Vector3 StartPosition { get; set; } = Vector3.zero;
    public Vector3 EndPosition { get; set; } = Vector3.zero;

    public Ease Ease { get; set; } = Ease.Linear;

    public override async UniTask AnimateAsync(CancellationToken token = default)
    {
        var rectTransform = View.RectTransform;
        rectTransform.anchoredPosition3D = StartPosition;

        // DOTween move + поддержка отмены через CancellationToken
        await rectTransform.DOAnchorPos3D(EndPosition, Duration)
            .SetEase(Ease)
            .WithCancellation(token);

        // (Не обязательно) итоговая позиция:
        rectTransform.anchoredPosition3D = EndPosition;
    }


    public MovingAnimation WithDuration(float duration)
    {
        this.Duration = duration;
        return this;
    }

    public MovingAnimation WithPath(Vector3 startPosition, Vector3 endPosition)
    {
        this.StartPosition = startPosition;
        this.EndPosition = endPosition;
        return this;
    }

    public MovingAnimation WithEase(Ease ease)
    {
        this.Ease = ease;
        return this;
    }
}