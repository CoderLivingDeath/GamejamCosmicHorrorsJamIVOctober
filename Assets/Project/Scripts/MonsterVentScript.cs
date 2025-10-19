using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Zenject;

public class MonsterVentScript : MonoBehaviour
{
    public float Duration = 10f;
    public Animator animator;
    public AudioSource audioSource;

    private Coroutine timerCoroutine;

    // События UnityEvent для начала и окончания таймера
    public UnityEvent OnTimerStarted;
    public UnityEvent OnTimerEnded;

    public void StartAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Start");
        }
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCoroutine());

        // Вызов события начала таймера
        OnTimerStarted?.Invoke();
    }

    public void CancleTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    public void PlaySound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    // Отменяет таймер и сразу заспавнит монстра
    public void CancleTimerAndSpawnMonster()
    {
        CancleTimer();
    }

    // Корутин таймера
    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(Duration);
        timerCoroutine = null;

        // Вызов события окончания таймера
        OnTimerEnded?.Invoke();
    }
}
