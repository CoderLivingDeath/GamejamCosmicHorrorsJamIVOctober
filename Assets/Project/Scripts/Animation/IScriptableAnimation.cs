using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IScriptableAnimation
{
    float Duration { get; }
    UniTask Run(CancellationToken token = default);

    public static IScriptableAnimation CreateAnimation(Action<float> action, float duration = 1.0f)
    {
        // Возвращаем интерфейс, создавая конкретный класс, реализующий IScriptableAnimation
        return new DelegateAnimation(action, duration);
    }

    public static IScriptableAnimation Combine(IScriptableAnimation left, IScriptableAnimation right, bool parallel = false)
    {
        // Создаем объект, возвращающий интерфейс
        return new CombineScriptableAnimation(left, right, parallel);
    }
}

public interface IScriptableAnimation<TContext>
{
    float Duration { get; }
    UniTask Run(TContext context, CancellationToken token = default);
    public static IScriptableAnimation<TContext> CreateAnimation(Action<float, TContext> action, float duration = 1.0f)
    {
        // Возвращаем интерфейс, создавая конкретный класс, реализующий IScriptableAnimation<TContext>
        return new DelegateAnimation<TContext>(action, duration);
    }
}