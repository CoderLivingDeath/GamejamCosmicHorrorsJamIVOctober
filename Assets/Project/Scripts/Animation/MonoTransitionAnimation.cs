using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MonoTransitionAnimation : MonoScriptableAnimation
{
    [SerializeField]
    private DoorOpenAnimation doorOpenAnimation;

    [SerializeField]
    private MonoPathAnimation characterMoveAnimation;

    [SerializeField]
    private DoorCloseAnimation doorCloseAnimation;

    public override float Duration
    {
        get => characterMoveAnimation.Duration + doorCloseAnimation.Duration + doorOpenAnimation.Duration;
        protected set => throw new NotSupportedException();
    }

    public override async UniTask Run(CancellationToken token = default)
    {
        await doorOpenAnimation.Run(token);
        await characterMoveAnimation.Run(token);
        doorCloseAnimation.Run(token).Forget();
    }
}