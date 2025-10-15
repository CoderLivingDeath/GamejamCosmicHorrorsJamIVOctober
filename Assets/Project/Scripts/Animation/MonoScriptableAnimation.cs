using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class MonoScriptableAnimation : MonoBehaviour, IScriptableAnimation
{
    public abstract float Duration { get; }

    public abstract UniTask Run(CancellationToken token = default);
}

public abstract class MonoScriptableAnimation<TContext> : MonoBehaviour, IScriptableAnimation<TContext>
{
    public abstract float Duration { get; }
    public abstract UniTask Run(TContext context, CancellationToken token = default);
}
