using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DoorOpenAnimation : MonoScriptableAnimation
{
    [SerializeField]
    private Animator _doorAnimator;

    [SerializeField]
    private string _openStateName = "Door_open";

    private float? _cachedDuration = null;

    public override float Duration
    {
        get
        {
            if (_cachedDuration.HasValue)
                return _cachedDuration.Value;

            var clips = _doorAnimator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == _openStateName)
                {
                    _cachedDuration = clip.length;
                    return _cachedDuration.Value;
                }
            }

            return 1.2f; // запасное значение
        }
        protected set => throw new System.NotSupportedException();
    }

    public override async UniTask Run(CancellationToken token = default)
    {
        await DoOpen(token);
    }

    public async UniTask DoOpen(CancellationToken token = default)
    {
        _doorAnimator.Play("Door_open", 0, 0f);

        float elapsed = 0f;
        float duration = Duration;

        while (elapsed < duration)
        {
            if (token.IsCancellationRequested)
                return;

            elapsed += Time.deltaTime;
            await UniTask.Yield(token);
        }
    }
}
