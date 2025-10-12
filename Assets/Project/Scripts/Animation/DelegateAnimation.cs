using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DelegateAnimation : IScriptableAnimation
{
    private readonly Action<float> _action;
    private readonly PlayerLoopTiming _timing;

    public float Duration { get; protected set; }


    public DelegateAnimation(Action<float> action, float duration, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        _action = action;
        Duration = duration;
        _timing = timing;
    }

    public async UniTask Run(CancellationToken token = default)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (token.IsCancellationRequested)
                return;

            float progress = elapsed / Duration;
            _action(progress);

            elapsed += Time.deltaTime;
            await UniTask.Yield(_timing, token);
        }
        // Финальный вызов с прогрессом 1
        _action(1f);
    }
}

public class DelegateAnimation<Context> : IScriptableAnimation
{
    private readonly Action<float, Context> _action;
    private readonly Context _context;
    private readonly PlayerLoopTiming _timing;
    public float Duration { get; protected set; }

    public DelegateAnimation(Action<float, Context> action, Context context, float duration, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        _action = action;
        _context = context;
        Duration = duration;
        _timing = timing;
    }

    public async UniTask Run(CancellationToken token = default)
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (token.IsCancellationRequested)
                return;

            float progress = elapsed / Duration;
            _action(progress, _context);

            elapsed += Time.deltaTime;
            await UniTask.Yield(_timing, token);
        }
        // Финальный вызов с прогрессом 1
        _action(1f, _context);
    }
}
