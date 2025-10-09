using System;
using UnityEngine;

public class AudioService
{
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SOUND_VOLUME_KEY = "SoundVolume";

    private readonly AudioSourcePool musicPool;
    private readonly AudioSourcePool soundPool;

    private readonly AudioConfigSO audioConfigSO;

    public AudioService(AudioSourcePool musicPool, AudioSourcePool soundPool, AudioConfigSO audioConfigSO)
    {
        this.musicPool = musicPool;
        this.soundPool = soundPool;
        this.audioConfigSO = audioConfigSO;
    }

    public float ToDecibel(float volume)
    {
        volume = Math.Clamp(volume, 0f, 1f);
        // Линейное преобразование нормализации в децибелы (-80..20)
        return volume * 100f - 80f;
    }

    public float ToNormalize(float decibel)
    {
        decibel = Math.Clamp(decibel, -80f, 20f);
        // Обратное линейное преобразование децибел в нормализованное значение (0..1)
        return (decibel + 80f) / 100f;
    }

    public AudioPlaybackScope PlayMusic(AudioClip clip)
    {
        if (clip == null) throw new ArgumentNullException(nameof(clip));

        if (musicPool.TryPlay(clip, out var scope))
            return scope;

        return null;
    }

    public AudioPlaybackScope PlaySound(AudioClip clip)
    {
        if (clip == null) throw new ArgumentNullException(nameof(clip));

        if (soundPool.TryPlay(clip, out var scope))
            return scope;

        return null;
    }


    public void SetMasterVolume(float volume)
    {
        var isFound = audioConfigSO.Master.audioMixer.SetFloat(MASTER_VOLUME_KEY, ToDecibel(volume));

        if (!isFound) Debug.LogWarning($"AudioMixer Parameter: {MASTER_VOLUME_KEY} - not found.");
    }

    public void SetMusicVolume(float volume)
    {
        var isFound = musicPool.MixerGroup.audioMixer.SetFloat(MUSIC_VOLUME_KEY, ToDecibel(volume));

        if (!isFound) Debug.LogWarning($"AudioMixer Parameter: {MUSIC_VOLUME_KEY} - not found.");
    }

    public void SetSoundVolume(float volume)
    {
        var isFound = musicPool.MixerGroup.audioMixer.SetFloat(SOUND_VOLUME_KEY, ToDecibel(volume));

        if (!isFound) Debug.LogWarning($"AudioMixer Parameter: {SOUND_VOLUME_KEY} - not found.");
    }

    public float GetMasterVolume()
    {
        audioConfigSO.Master.audioMixer.GetFloat(MASTER_VOLUME_KEY, out var volume);
        return ToNormalize(volume);
    }

    public float GetMusicVolume()
    {
        musicPool.MixerGroup.audioMixer.GetFloat(MUSIC_VOLUME_KEY, out var volume);
        return ToNormalize(volume);
    }

    public float GetSoundVolume()
    {
        soundPool.MixerGroup.audioMixer.GetFloat(SOUND_VOLUME_KEY, out var volume);
        return ToNormalize(volume);
    }
}
