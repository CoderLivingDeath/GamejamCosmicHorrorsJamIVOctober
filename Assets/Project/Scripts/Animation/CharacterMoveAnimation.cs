using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using EditorAttributes;

public class CharacterMoveAnimation : MonoScriptableAnimation
{
    [SerializeField]
    private float _duration = 1.0f;
    public override float Duration { get => _duration; protected set => _duration = value; }

    public MonoCharacter Character;
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public Ease Ease;

    public override async UniTask Run(CancellationToken token = default)
    {
        Character.Transform.position = StartPosition;

        Tween tween = DOTween.To(() => Character.Transform.position,
            x =>
            {
                Vector3 newPos = x;
                Character.Transform.position = newPos;
                Character.SnapToSurface();
            },
            EndPosition,
            Duration).SetEase(Ease);

        await tween.ToUniTask(cancellationToken: token);
    }
}