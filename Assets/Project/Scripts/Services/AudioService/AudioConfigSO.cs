using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "AudioConfigSO", menuName = "Configs/AudioConfig")]
public class AudioConfigSO : ScriptableObject
{
    public int SoundPoolSize = 8;
    public int MusicPoolSize = 8;

    public AudioMixer audioMixer;
    public AudioMixerGroup Master;
    public AudioMixerGroup SoundGroup;
    public AudioMixerGroup MusicGroup;
}