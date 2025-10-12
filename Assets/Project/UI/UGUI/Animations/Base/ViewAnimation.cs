using Cysharp.Threading.Tasks;
using System;
using System.Threading;

public abstract class ViewAnimation<T> : IViewAnimation where T : MonoCanvasView
{
    public T View { get; }

    public float Duration { get; protected set; } = 1f;

    MonoCanvasView IViewAnimation.View => View;

    protected ViewAnimation(T view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));
    }

    public abstract UniTask Run(CancellationToken token = default);

}
