using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MonoScriptableAnimationFromAnimationTrigger : MonoScriptableAnimation
{
    [SerializeField] private Animator animator;
    public override float Duration => 1f;

    public string Trigger;

    //TODO: вызывает баги. необходимо дожидаться окончания анимации перед запуском следующей.
    //TODO: Исправить ожидание. ожидание Происходит по времени Duration, что может не соответсвовать реальной анимации.
    public override async UniTask Run(CancellationToken token = default)
    {
        if (animator == null)
            throw new InvalidOperationException("Animator is null");

        if (string.IsNullOrEmpty(Trigger))
            throw new InvalidOperationException("Trigger is null or empty");

        animator.SetTrigger(Trigger);

        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (token.IsCancellationRequested)
                return;

            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

}
