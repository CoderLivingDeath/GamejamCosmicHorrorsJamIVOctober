
using UnityEngine;

public class RandomSoundPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    // Этот метод вызывается из анимации через Animation Event
    public void PlayRandomSound()
    {
        if (clips == null || clips.Length == 0 || audioSource == null)
            return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        audioSource.PlayOneShot(clip);
    }
    public void DummyEvent() { }
}