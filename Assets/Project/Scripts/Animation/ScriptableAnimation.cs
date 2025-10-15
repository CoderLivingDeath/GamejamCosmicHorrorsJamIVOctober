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
        return new DelegateAnimation<Context>(action, duration);
    }

    public static IScriptableAnimation Combine(IScriptableAnimation left, IScriptableAnimation right, bool parallel = false)
    {
        return new CombineScriptableAnimation(left, right, parallel);
    }
}
