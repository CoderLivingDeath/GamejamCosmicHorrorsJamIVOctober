using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using EditorAttributes;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Zenject;

public class MonoDoorTransition : MonoBehaviour
{
    public MonoPathAnimation pathAnimation;

    public MonoScriptableAnimation DoorOpen;
    public MonoScriptableAnimation DoorClose;

    public UnityEvent OnEnd;

    public IScriptableAnimationScope Animate(MonoCharacterController monoCharacterController, bool pathAnimationReverce = false)
    {
        float duration = pathAnimation.Duration;
        string walkKey = "Walk";
        string nextKey = "Idle";

        var transitAnimation = new DoorTransitionAnomationWithCharacter(duration, walkKey, nextKey, DoorOpen, DoorClose, pathAnimation, pathAnimationReverce: pathAnimationReverce);

        var scope = monoCharacterController.PlayerAnimationWithAnimatorAndTransform(transitAnimation);
        scope.Completed += () =>
        {
            OnEnd.Invoke();
        };

        return scope;
    }
}

public sealed class DoorTransitionAnomationWithCharacter : IScriptableAnimation<(Animator, Transform)>
{
    public DoorTransitionAnomationWithCharacter(float duration, string walkKey, string nextKey,
     MonoScriptableAnimation doorOpen,
     MonoScriptableAnimation doorClose,
     MonoPathAnimation pathAnimation,
     PlayerLoopTiming timing = PlayerLoopTiming.Update, bool pathAnimationReverce = false)
    {
        this.pathAnimation = pathAnimation;
        Duration = duration;
        this.WalkKey = walkKey;
        this.NextKey = nextKey;
        Timing = timing;
        DoorOpen = doorOpen;
        DoorClose = doorClose;
        PathAnimationReverce = pathAnimationReverce;
    }

    public MonoScriptableAnimation DoorOpen { get; private set; }
    public MonoScriptableAnimation DoorClose { get; private set; }
    public MonoPathAnimation pathAnimation { get; private set; }
    public string WalkKey { get; private set; }
    public string NextKey { get; private set; }

    public bool PathAnimationReverce { get; private set; }

    public float Duration { get => pathAnimation.Duration; private set => pathAnimation.SetDuration(value); }

    public PlayerLoopTiming Timing { get; private set; }

    public async UniTask Run((Animator, Transform) context, CancellationToken token = default)
    {
        var animator = context.Item1;
        var transform = context.Item2;

        await DoorOpen.Run(token);

        animator.SetTrigger(WalkKey);

        await pathAnimation.PlayPath(transform, PathAnimationReverce, token);

        DoorClose.Run(token).Forget();

        // animator.SetTrigger(NextKey); // TODO: проблема, ломается перснаж если держать кнопку передвижения во время и после прохода через дверь
    }
}