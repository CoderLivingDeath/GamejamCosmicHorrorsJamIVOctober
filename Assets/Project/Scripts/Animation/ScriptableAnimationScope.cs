using System;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

// В будущем возможны оптимизации с использованием ref struct
public class ScriptableAnimationScope : IDisposable
{
    public enum Status
    {
        NotStarted, Started, Cancelled, Compleated
    }

    public readonly IScriptableAnimation Animation;
    public readonly UniTask Task;
    private readonly Stopwatch _watch;
    private readonly CancellationTokenSource _cts;

    public event Action Completed;
    public event Action Canceled;

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
    public TimeSpan Elapsed => _watch.Elapsed;

    /// <summary>
    /// Оставшееся время для завершения анимации.
    /// </summary>
    public TimeSpan Remaining
    {
        get
        {
            var durationTs = TimeSpan.FromSeconds(DurationSeconds);
            var left = durationTs - _watch.Elapsed;
            return left > TimeSpan.Zero ? left : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// Токен отмены анимации.
    /// </summary>
    public CancellationToken Token => _cts.Token;

    public ScriptableAnimationScope(IScriptableAnimation animation, CancellationToken externalToken = default)
    {
        Animation = animation ?? throw new ArgumentNullException(nameof(animation));
        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _watch = new Stopwatch();
        _status = Status.NotStarted;

        Task = AnimateWithCatchAsync(_cts.Token);
    }

    private async UniTask AnimateWithCatchAsync(CancellationToken token)
    {
        try
        {
            _status = Status.Started;
            _watch.Start();

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

    public async UniTask WaitForEnd()
    {
        ThrowIfDisposed();
        try
        {
            await Task.AttachExternalCancellation(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Отмена операции
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
                // Отмена операции
            }
        }
    }

    /// <summary>
    /// Ожидание оставшегося времени анимации.
    /// </summary>
    public async UniTask WaitForRemaining()
    {
        await WaitForTime(Remaining);
    }

    public void Cancel()
    {
        ThrowIfDisposed();
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            Canceled?.Invoke();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        Cancel();
        _cts.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ScriptableAnimationScope));
    }
}
