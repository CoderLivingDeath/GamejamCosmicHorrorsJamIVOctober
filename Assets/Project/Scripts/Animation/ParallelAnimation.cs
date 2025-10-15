using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class ParallelAnimation : MonoScriptableAnimation
{
    [SerializeField]
    private MonoScriptableAnimation[] _animations;

    public override float Duration
    {
        get
        {
            if (_animations == null || _animations.Length == 0)
                return 0f;

            float maxDuration = 0f;
            foreach (var anim in _animations)
            {
                if (anim != null && anim.Duration > maxDuration)
                    maxDuration = anim.Duration;
            }
            return maxDuration;
        }
    }

    public override async UniTask Run(CancellationToken token = default)
    {
        if (_animations == null || _animations.Length == 0)
            return;

        var tasks = new UniTask[_animations.Length];
        for (int i = 0; i < _animations.Length; i++)
        {
            if (_animations[i] != null)
                tasks[i] = _animations[i].Run(token);
            else
                tasks[i] = UniTask.CompletedTask;
        }
        await UniTask.WhenAll(tasks);
    }
}