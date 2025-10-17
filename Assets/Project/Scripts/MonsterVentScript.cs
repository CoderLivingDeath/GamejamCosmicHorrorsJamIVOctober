using UnityEngine;
using System.Collections;
using Zenject;

public class MonsterVentScript : MonoBehaviour
{
    [Inject]
    private MonsterSpawner monsterSpawner;

    public float Duration = 10f;
    public Animator animator;
    public AudioSource audioSource;

    public Transform SpawnPoint;

    private Coroutine timerCoroutine;

    public void StartAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Start");
        }
    }

    public void SpawnMonster()
    {
        monsterSpawner.Spawn(SpawnPoint.position);
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        timerCoroutine = StartCoroutine(TimerCoroutine());
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
        SpawnMonster();
    }

    // Корутин таймера
    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(Duration);
        SpawnMonster();
        timerCoroutine = null;
    }
}
