using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DoorCloseAnimation : MonoScriptableAnimation
{
    [SerializeField]
    private Animator _doorAnimator;

    [SerializeField]
    private string _closeStateName = "Door_close";
    private float? _cachedDuration = null;

    // Получаем длительность из AnimationClip с именем _closeStateName
    public override float Duration
    {
        get
        {
            if (_cachedDuration.HasValue)
                return _cachedDuration.Value;

            // Ищем соответствующий AnimationClip по имени
            var clips = _doorAnimator.runtimeAnimatorController.animationClips;
            foreach (var clip in clips)
            {
                if (clip.name == _closeStateName)
                {
                    _cachedDuration = clip.length;
                    return _cachedDuration.Value;
                }
            }

            // Если не нашли, вернем некоторую дефолтную длительность
            return 1.2f;
        }
        protected set => throw new System.NotSupportedException();
    }

    public override async UniTask Run(CancellationToken token = default)
    {
        await DoClose(token);
    }

    public async UniTask DoClose(CancellationToken token = default)
    {
        _doorAnimator.Play("Door_close", 0, 0f);

        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (token.IsCancellationRequested)
                return;
            elapsed += Time.deltaTime;
            await UniTask.Yield(token);
        }
    }
}