using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CombineScriptableAnimation : IScriptableAnimation
{
    public float Duration => Parallel
        ? Math.Max(Left.Duration, Right.Duration)
        : Left.Duration + Right.Duration;

    public IScriptableAnimation Left { get; protected set; }
    public IScriptableAnimation Right { get; protected set; }

    public bool Parallel { get; protected set; }

    public CombineScriptableAnimation(IScriptableAnimation left, IScriptableAnimation right, bool parallel = false)
    {
        Parallel = parallel;
        Left = left;
        Right = right;
    }

    public async UniTask Run(CancellationToken token = default)
    {
        if (Parallel)
        {
            // Запускать обе анимации параллельно и ждать завершения обеих
            await UniTask.WhenAll(Left.Run(token), Right.Run(token));
        }
        else
        {
            // Запускать последовательно: сначала Left, потом Right
            await Left.Run(token);
            await Right.Run(token);
        }
    }
}

public class CombineScriptableAnimation<TContext> : IScriptableAnimation<TContext>
{
    public float Duration => Parallel
        ? Math.Max(Left.Duration, Right.Duration)
        : Left.Duration + Right.Duration;

    public IScriptableAnimation<TContext> Left { get; protected set; }
    public IScriptableAnimation<TContext> Right { get; protected set; }

    public bool Parallel { get; protected set; }

    public CombineScriptableAnimation(IScriptableAnimation<TContext> left, IScriptableAnimation<TContext> right, bool parallel = false)
    {
        Parallel = parallel;
        Left = left;
        Right = right;
    }

    public async UniTask Run(TContext context, CancellationToken token = default)
    {
        if (Parallel)
        {
            // Запускаем обе анимации одновременно и ждём обеих
            await UniTask.WhenAll(Left.Run(context, token), Right.Run(context, token));
        }
        else
        {
            // Запускаем поочерёдно
            await Left.Run(context, token);
            if (token.IsCancellationRequested)
                return;
            await Right.Run(context, token);
        }
    }

}
