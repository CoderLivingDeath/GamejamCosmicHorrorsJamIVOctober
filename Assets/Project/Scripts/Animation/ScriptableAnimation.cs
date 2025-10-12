using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public abstract class ScriptableAnimation : IScriptableAnimation
{
    public abstract float Duration { get; protected set; }

    public abstract UniTask Run(CancellationToken token = default);

    public static DelegateAnimation CreateAnimation(Action<float> action, float duration = 1.0f)
    {
        return new DelegateAnimation(action, duration);
    }

    public static DelegateAnimation<Context> CreateAnimation<Context>(Action<float, Context> action, Context context, float duration = 1.0f)
    {
        return new DelegateAnimation<Context>(action, context, duration);
    }
}