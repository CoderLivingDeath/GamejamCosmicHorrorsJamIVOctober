using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPlaybackScope : IDisposable
{
    public readonly AudioClip Clip;

    public bool IsActive => audioSource.enabled && audioSource.isPlaying;
    public float Duration => Clip.length;

    public float CurrentTime => audioSource.isPlaying ? audioSource.time : 0f;

    public event Action OnPlaybackStarted;
    public event Action OnPlaybackEnded;

    private readonly AudioSource audioSource;
    private readonly AudioMixerGroup group;

    private bool disposed;

    public AudioPlaybackScope(AudioSource source, AudioClip clip, AudioMixerGroup mixerGroup = null)
    {
        audioSource = source ?? throw new ArgumentNullException(nameof(source));
        Clip = clip ?? throw new ArgumentNullException(nameof(clip));
        group = mixerGroup;

        PlayClip();
    }

    private void PlayClip()
    {
        audioSource.clip = Clip;
        audioSource.outputAudioMixerGroup = group;

        audioSource.Play();

        OnPlaybackStarted?.Invoke();
    }

    public void Stop()
    {
        if (!IsActive)
            return;

        audioSource.Stop();
        OnPlaybackEnded?.Invoke();
    }

    public float GetRemainingTime()
    {
        if (!IsActive || !audioSource.isPlaying)
            return 0f;

        return Mathf.Max(0f, Duration - audioSource.time);
    }

    public async UniTask WaitForPlaybackEndAsync(CancellationToken cancellationToken = default)
    {
        if (!IsActive)
            return;

        try
        {
            while (audioSource.isPlaying)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        }
        catch (OperationCanceledException)
        {
            // Ожидание отменено, если нужна обработка
        }
        finally
        {
            OnPlaybackEnded?.Invoke();
        }
    }

    public void Dispose()
    {
        if (disposed)
            return;

        Stop();
        disposed = true;
    }
}