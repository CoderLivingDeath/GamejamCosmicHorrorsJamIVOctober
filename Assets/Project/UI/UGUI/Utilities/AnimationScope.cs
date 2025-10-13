using Cysharp.Threading.Tasks;
using System;
using System.Diagnostics;
using System.Threading;

public class AnimationScope : IDisposable
{
    private readonly IViewAnimation _animation;
    private readonly UniTask _task;
    private readonly Stopwatch _watch;
    private readonly CancellationTokenSource _cts;

    public event Action Completed;
    public event Action Canceled;

    private bool _disposed;

    /// <summary>
    /// ����� ����� �������� (�������) �� ����������.
    /// </summary>
    public float DurationSeconds => _animation.Duration;

    /// <summary>
    /// ��������� � ������ �������� �����.
    /// </summary>
    public TimeSpan Elapsed => _watch.Elapsed;

    /// <summary>
    /// ���������� ����� �������� � ������ Duration.
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

    public CancellationToken Token => _cts.Token;

    public AnimationScope(IViewAnimation animation, CancellationToken externalToken = default)
    {
        _animation = animation ?? throw new ArgumentNullException(nameof(animation));
        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        _watch = Stopwatch.StartNew();

        _task = AnimateWithCatchAsync(_cts.Token);
    }

    private async UniTask AnimateWithCatchAsync(CancellationToken token)
    {
        _animation.View.IsAnimating = true;
        try
        {
            await _animation.Run(token);
            Completed?.Invoke();
        }
        catch (OperationCanceledException)
        {
            // ������ � ������
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Animation failed: {ex}");
            throw;
        }
        finally
        {
            _animation.View.IsAnimating = false;
        }
    }


    public async UniTask WaitForEnd()
    {
        ThrowIfDisposed();
        try
        {
            await _task.AttachExternalCancellation(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // ������ � ������
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
                // ������ � ������
            }
        }
    }

    /// <summary>
    /// ��������� ���������� ����� ��������.
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
            throw new ObjectDisposedException(nameof(AnimationScope));
    }
}
