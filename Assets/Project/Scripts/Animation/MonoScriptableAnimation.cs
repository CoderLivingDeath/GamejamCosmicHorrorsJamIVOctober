using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class MonoScriptableAnimation : MonoBehaviour, IScriptableAnimation
{
    public abstract float Duration { get; protected set; }

    public abstract UniTask Run(CancellationToken token = default);
}


public class ScriptableAnimationWithEvents : IScriptableAnimation
{
    private readonly IScriptableAnimation _animation;

    public event Action OnStart;
    public event Action OnEnd;
    public event Action OnCanceled;

    public float Duration => _animation.Duration;

    public ScriptableAnimationWithEvents(IScriptableAnimation animation)
    {
        _animation = animation ?? throw new ArgumentNullException(nameof(animation));
    }

    public async UniTask Run(CancellationToken token = default)
    {
        OnStart?.Invoke();

        try
        {
            await _animation.Run(token);
            if (token.IsCancellationRequested)
            {
                OnCanceled?.Invoke();
            }
            else
            {
                OnEnd?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            OnCanceled?.Invoke();
        }
    }
}
