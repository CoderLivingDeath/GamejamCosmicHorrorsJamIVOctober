using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;

public class QueenScriptableAnimation : IScriptableAnimation
{
    private readonly Queue<IScriptableAnimation> _animationsQueue;

    public float Duration
    {
        get
        {
            return _animationsQueue.Sum(anim => anim.Duration);
        }
    }

    public QueenScriptableAnimation(IEnumerable<IScriptableAnimation> animations)
    {
        if (animations == null)
            throw new ArgumentNullException(nameof(animations));

        _animationsQueue = new Queue<IScriptableAnimation>(animations);
    }

    public async UniTask Run(CancellationToken token = default)
    {
        while (_animationsQueue.Count > 0 && !token.IsCancellationRequested)
        {
            token.ThrowIfCancellationRequested();

            IScriptableAnimation nextAnimation = _animationsQueue.Dequeue();

            // Запускаем следующую анимацию, ждём её завершения
            await nextAnimation.Run(token);
        }
    }
}
