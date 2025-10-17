using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IScriptableAnimationScope
{
    ScriptableAnimationScope.Status CurrentStatus { get; }
    float DurationSeconds { get; }
    TimeSpan Elapsed { get; }
    TimeSpan Remaining { get; }
    CancellationToken Token { get; }

    event Action OnStart;
    event Action Completed;
    event Action Canceled;

    void Cancel();
    void Dispose();
    UniTask WaitForEnd();
    UniTask WaitForRemaining();
    UniTask WaitForTime(TimeSpan duration);
}

public class ScriptableAnimationScope : IDisposable, IScriptableAnimationScope
{
    public enum Status
    {
        NotStarted, Started, Cancelled, Compleated
    }

    public readonly IScriptableAnimation Animation;
    private CancellationTokenSource _cts;
    private Stopwatch _watch;

    private bool _disposed;
    private Status _status;

    public Status CurrentStatus => _status;

    /// <summary>
    /// Длительность анимации в секундах.
    /// </summary>
    public float DurationSeconds => Animation.Duration;

    /// <summary>
    /// Прошедшее время с начала анимации.
    /// </summary>
    public TimeSpan Elapsed => _watch?.Elapsed ?? TimeSpan.Zero;

    /// <summary>
    /// Оставшееся время для завершения анимации.
    /// </summary>
    public TimeSpan Remaining
    {
        get
        {
            var durationTs = TimeSpan.FromSeconds(DurationSeconds);
            var left = durationTs - Elapsed;
            return left > TimeSpan.Zero ? left : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Токен отмены анимации.
    /// </summary>
    public CancellationToken Token => _cts?.Token ?? CancellationToken.None;

    public event Action OnStart;
    public event Action Completed;
    public event Action Canceled;

    private CancellationToken _externalToken;

    public ScriptableAnimationScope(IScriptableAnimation animation, CancellationToken externalToken = default)
    {
        Animation = animation ?? throw new ArgumentNullException(nameof(animation));
        _externalToken = externalToken;
        _status = Status.NotStarted;
    }

    /// <summary>
    /// Запустить анимацию, вернуть задачу для ожидания завершения.
    /// </summary>
    public UniTask Start()
    {
        ThrowIfDisposed();

        if (_status != Status.NotStarted)
            throw new InvalidOperationException("Animation already started");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(_externalToken);
        _watch = new Stopwatch();
        _status = Status.Started;
        _watch.Start();

        OnStart?.Invoke();

        return AnimateWithCatchAsync(_cts.Token);
    }

    private async UniTask AnimateWithCatchAsync(CancellationToken token)
    {
        try
        {
            await Animation.Run(token);

            _status = Status.Compleated;
            Completed?.Invoke();
        }
        catch (OperationCanceledException)
        {
            _status = Status.Cancelled;
            Canceled?.Invoke();
        }
        catch (Exception)
        {
            _status = Status.Cancelled;
            throw;
        }
        finally
        {
            _watch.Stop();
        }
    }

    // Остальные методы без изменений...

    public async UniTask WaitForEnd()
    {
        ThrowIfDisposed();
        try
        {
            await AnimateWithCatchAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Отмена
        }
    }

    public async UniTask WaitForTime(TimeSpan duration)
    {
        ThrowIfDisposed();
        var left = duration - _watch.Elapsed;
        if (left > TimeSpan.Zero)
        {
            try
            {
                await UniTask.Delay(left, cancellationToken: _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Отмена
            }
        }
    }

    public async UniTask WaitForRemaining()
    {
        await WaitForTime(Remaining);
    }

    public void Cancel()
    {
        ThrowIfDisposed();
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            Canceled?.Invoke();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        Cancel();
        _cts?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ScriptableAnimationScope));
    }
}
