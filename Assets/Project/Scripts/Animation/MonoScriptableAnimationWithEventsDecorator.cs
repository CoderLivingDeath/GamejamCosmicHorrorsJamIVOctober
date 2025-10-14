using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public sealed class MonoScriptableAnimationWithEventsDecorator : MonoScriptableAnimation
{
    public MonoScriptableAnimation Animation => _animation;

    [SerializeField]
    private MonoScriptableAnimation _animation;

    public UnityEvent OnStart;
    public UnityEvent OnEnd;
    public UnityEvent OnCanceled;

    public override float Duration
    {
        get
        {
            if (_animation == null) throw new NullReferenceException("Animation is null.");
            return _animation.Duration;
        }
        protected set => throw new NotSupportedException("Duration property is read-only.");
    }

    public override async UniTask Run(CancellationToken token = default)
    {
        if (_animation == null)
        {
            OnCanceled?.Invoke();
            return;
        }

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