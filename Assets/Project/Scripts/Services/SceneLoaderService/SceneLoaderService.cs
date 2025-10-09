using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public interface ISceneLoaderService
{
    LoadingSceneScope LoadScene(int index);
}

public class SceneLoaderService : ISceneLoaderService
{
    readonly ZenjectSceneLoader _loader;

    public SceneLoaderService(ZenjectSceneLoader loader)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
    }

    public LoadingSceneScope LoadScene(int index)
    {
        return new LoadingSceneScope(index, _loader);
    }
}

public class LoadingSceneScope : IDisposable
{
    private AsyncOperation _asyncOperation;
    private readonly CancellationTokenSource _cts;
    private UniTaskCompletionSource<float> _progressSource;
    private UniTaskCompletionSource<bool> _completedSource;
    private readonly UniTask _loadingTask;
    private bool _isDisposed;

    public float Progress => _asyncOperation?.progress ?? 0f;
    public bool IsDone => _asyncOperation != null && _asyncOperation.isDone;

    public event Action<float> ProgressChanged;
    public event Action LoadingCompleted;
    public event Action SceneFullyLoaded;

    public LoadingSceneScope(int sceneIndex, ZenjectSceneLoader loader)
    {
        if (loader == null) throw new ArgumentNullException(nameof(loader));

        _cts = new CancellationTokenSource();
        _progressSource = new UniTaskCompletionSource<float>();  // Инициализация здесь
        _completedSource = new UniTaskCompletionSource<bool>();  // Инициализация здесь
        _loadingTask = LoadSceneAsync(sceneIndex, loader);
    }

    private async UniTask LoadSceneAsync(int sceneIndex, ZenjectSceneLoader loader)
    {
        _asyncOperation = loader.LoadSceneAsync(sceneIndex);
        _asyncOperation.allowSceneActivation = false;

        bool loadingCompletedCalled = false;
        float lastProgress = -1f;  // Начальное значение для первого вызова

        while (!_asyncOperation.isDone)
        {
            if (_cts.Token.IsCancellationRequested)
                break;

            float currentProgress = _asyncOperation.progress;

            // Вызываем только если прогресс изменился
            if (Math.Abs(currentProgress - lastProgress) > 0.001f)
            {
                ProgressChanged?.Invoke(currentProgress);
                _progressSource?.TrySetResult(currentProgress);
                lastProgress = currentProgress;
            }

            if (currentProgress >= 0.9f && !loadingCompletedCalled)
            {
                LoadingCompleted?.Invoke();
                _completedSource?.TrySetResult(true);
                loadingCompletedCalled = true;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, _cts.Token);
        }

        // Вызов финального события после полной загрузки
        if (!_cts.Token.IsCancellationRequested)
        {
            SceneFullyLoaded?.Invoke();
        }
    }

    public void ActivateScene()
    {
        if (_asyncOperation != null && !_asyncOperation.allowSceneActivation)
            _asyncOperation.allowSceneActivation = true;
        else
            Debug.LogWarning("No scene loaded to activate or already activated.");
    }

    public UniTask<float> WaitProgressAsync()
    {
        return _progressSource.Task;
    }

    public UniTask WaitCompletedAsync()
    {
        return _completedSource.Task;
    }

    public void CancelLoading()
    {
        _cts.Cancel();
    }

    public UniTask WaitUntilLoadedAsync()
    {
        return _loadingTask;
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _cts.Cancel();
        _cts.Dispose();
        _isDisposed = true;
    }
}
