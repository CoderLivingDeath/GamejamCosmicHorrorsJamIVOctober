using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
/// <summary>
/// Central event bus with thread-safety, async support, and WeakReference for subscribers to prevent memory leaks.
/// </summary>
public class EventBus
{
    // Thread-safe dictionary for subscribers
    private readonly ConcurrentDictionary<Type, SubscribersList<IGlobalSubscriber>> s_Subscribers
        = new ConcurrentDictionary<Type, SubscribersList<IGlobalSubscriber>>();

    /// <summary>
    /// Subscribes a global subscriber to all event interfaces it implements (using WeakReference).
    /// </summary>
    public void Subscribe(IGlobalSubscriber subscriber)
    {
        var subscriberTypes = EventBusHelper.GetSubscriberTypes(subscriber);
        foreach (var t in subscriberTypes)
        {
            var list = s_Subscribers.GetOrAdd(t, _ => new SubscribersList<IGlobalSubscriber>());
            list.Add(subscriber);
        }
    }

    /// <summary>
    /// Unsubscribes a global subscriber from all event interfaces it implements.
    /// </summary>
    public void Unsubscribe(IGlobalSubscriber subscriber)
    {
        var subscriberTypes = EventBusHelper.GetSubscriberTypes(subscriber);
        foreach (var t in subscriberTypes)
        {
            if (s_Subscribers.TryGetValue(t, out var list))
            {
                list.Remove(subscriber);
            }
        }
    }

    /// <summary>
    /// Raises an event synchronously for all subscribers of the given type.
    /// </summary>
    public void RaiseEvent<TSubscriber>(Action<TSubscriber> action)
        where TSubscriber : class, IGlobalSubscriber
    {
        if (!s_Subscribers.TryGetValue(typeof(TSubscriber), out var subscribers) || subscribers == null)
        {
            return;
        }

        subscribers.Executing = true;
        foreach (var weakRef in subscribers.GetSnapshot())
        {
            if (weakRef.TryGetTarget(out var target))
            {
                try
                {
                    action.Invoke(target as TSubscriber);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
        }
        subscribers.Executing = false;
        subscribers.Cleanup();
    }

    public async UniTask RaiseEventAsync<TSubscriber>(Func<TSubscriber, UniTask> func, SynchronizationContext ctx = null)
            where TSubscriber : class, IGlobalSubscriber
    {
        if (!s_Subscribers.TryGetValue(typeof(TSubscriber), out var subs) || subs == null)
            return;

        subs.Executing = true;
        var tasks = new List<UniTask>();
        ctx ??= SynchronizationContext.Current;  // Используем переданный или текущий контекст

        // Локальная функция-обёртка: выполняет func в нужном контексте
        UniTask InvokeOnContext(TSubscriber target)
        {
            // Уже в нужном контексте — вызываем напрямую
            if (ctx == null || ctx == SynchronizationContext.Current)
                return func(target);

            // Иначе постим в указанный контекст и возвращаем UniTask, который завершится после func
            var utcs = new UniTaskCompletionSource();
            ctx.Post(async _ =>
            {
                try
                {
                    await func(target);
                    utcs.TrySetResult();
                }
                catch (Exception e)
                {
                    utcs.TrySetException(e);
                    Debug.LogError(e);
                }
            }, null);
            return utcs.Task;
        }

        // Собираем задачи со всех живых подписчиков
        foreach (var weakRef in subs.GetSnapshot())
            if (weakRef.TryGetTarget(out var t))
                tasks.Add(InvokeOnContext(t as TSubscriber));

        await UniTask.WhenAll(tasks);
        subs.Executing = false;
        subs.Cleanup();
    }
}

internal static class EventBusHelper
{
    private static readonly ConcurrentDictionary<Type, List<Type>> s_CachedSubscriberTypes
        = new ConcurrentDictionary<Type, List<Type>>();

    public static List<Type> GetSubscriberTypes(IGlobalSubscriber globalSubscriber)
    {
        var type = globalSubscriber.GetType();
        return s_CachedSubscriberTypes.GetOrAdd(type, t =>
            t.GetInterfaces()
                .Where(i => typeof(IGlobalSubscriber).IsAssignableFrom(i) && i != typeof(IGlobalSubscriber))
                .ToList());
    }
}

internal class SubscribersList<TSubscriber> where TSubscriber : class
{
    private readonly object _lock = new object();
    private bool _needsCleanup = false;
    public bool Executing;
    private readonly List<WeakReference<TSubscriber>> _list = new List<WeakReference<TSubscriber>>();

    public void Add(TSubscriber subscriber)
    {
        if (subscriber == null) return;
        lock (_lock)
        {
            // Prevent duplicates
            if (!_list.Any(w => w.TryGetTarget(out var target) && ReferenceEquals(target, subscriber)))
            {
                _list.Add(new WeakReference<TSubscriber>(subscriber));
            }
        }
    }

    public void Remove(TSubscriber subscriber)
    {
        if (subscriber == null) return;
        lock (_lock)
        {
            if (Executing)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    if (_list[i].TryGetTarget(out var target) && ReferenceEquals(target, subscriber))
                    {
                        _needsCleanup = true;
                        _list[i] = null; // Mark for cleanup
                        break;
                    }
                }
            }
            else
            {
                _list.RemoveAll(w => w.TryGetTarget(out var target) && ReferenceEquals(target, subscriber));
            }
        }
    }

    public void Cleanup()
    {
        lock (_lock)
        {
            _list.RemoveAll(w => w == null || !w.TryGetTarget(out _));
            _needsCleanup = false;
        }
    }

    // Returns a snapshot for safe iteration during execution
    public WeakReference<TSubscriber>[] GetSnapshot()
    {
        lock (_lock)
        {
            return _list.ToArray();
        }
    }
}
