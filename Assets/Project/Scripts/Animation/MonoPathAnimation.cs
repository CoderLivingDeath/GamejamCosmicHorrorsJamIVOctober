using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using EditorAttributes;

public class MonoPathAnimation : MonoScriptableAnimation
{
    public override float Duration { get => _duration; protected set => _duration = value; }
    
    [SerializeField]
    private float _duration = 1f;

    public Vector3[] Points;

    public Transform transformToAnimate; // объект, который двигаем

    public enum CoordinateSpace
    {
        World,
        LocalToMonoPathAnimation
    }

    public LayerMask BoundsCollisionLayers = ~0; // По умолчанию все слои
    public CoordinateSpace coordinateSpace = CoordinateSpace.LocalToMonoPathAnimation;

    public PathType pathType = PathType.Linear;

    public Ease Ease = Ease.Linear;

    [Header("Bounds Support")]
    public bool useBoundsSupport = false;

    public Vector3 boundsSize = Vector3.one;

    public Collider[] boundsOverlapCheckColliders = new Collider[0];

    public override async UniTask Run(CancellationToken token = default)
    {
        if (Points == null || Points.Length == 0)
            throw new InvalidOperationException("Points array is null or empty");

        if (transformToAnimate == null)
            throw new InvalidOperationException("Transform to animate is null");

        Vector3[] pathPoints;

        if (coordinateSpace == CoordinateSpace.LocalToMonoPathAnimation)
        {
            // Преобразуем локальные точки относительно MonoPathAnimation в мировые
            pathPoints = new Vector3[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                pathPoints[i] = this.transform.TransformPoint(Points[i]);
            }
        }
        else
        {
            pathPoints = Points;
        }

        Tween pathTween = transformToAnimate.DOPath(pathPoints, Duration, pathType)
            .SetOptions(false)
            .SetEase(Ease);

        await pathTween.ToUniTask(cancellationToken: token);
    }

    [Button]
    public void RunAnimationButton()
    {
        Run().Forget();
    }
}
