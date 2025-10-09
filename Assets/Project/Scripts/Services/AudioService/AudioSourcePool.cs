using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSourcePool
{
    public int Count { get; private set; }

    public AudioMixerGroup MixerGroup => _mixerGroup;

    public IEnumerable<AudioSource> AudioSources => _sources;

    public IEnumerable<AudioSource> FreeSources => _freeSources;

    private readonly AudioMixerGroup _mixerGroup;
    private readonly IEnumerable<AudioSource> _sources;
    private IEnumerable<AudioSource> _freeSources => _sources.Where(x => x != null && x.enabled && !x.isPlaying);

    public AudioSourcePool(IEnumerable<AudioSource> sources, AudioMixerGroup mixerGroup = null, int count = 8)
    {
        Count = count;
        _mixerGroup = mixerGroup;
        _sources = sources;
    }

    public bool TryPlay(AudioClip clip, out AudioPlaybackScope playbackScope)
    {
        playbackScope = null;
        if (_freeSources.Count() == 0)
        {
            // Optionally expand pool here or return false
            return false;
        }

        var source = _freeSources.FirstOrDefault();
        playbackScope = new AudioPlaybackScope(source, clip, _mixerGroup);
        return true;
    }

    public void StopAll()
    {
        foreach (var source in _sources)
        {
            source.Stop();
        }
    }
}
