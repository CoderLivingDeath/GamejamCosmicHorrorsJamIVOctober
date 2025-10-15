using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorAttributes;
using UnityEngine;

public class MonoPathAnimation : MonoScriptableAnimation<Transform>
{
    public enum CoordinateSpace
    {
        World,
        LocalToMonoPathAnimation
    }

    [SerializeField] private float _duration = 1f;
    public Vector3[] Points;
    [SerializeField] private Transform transformToAnimate;
    public LayerMask BoundsCollisionLayers = ~0;
    public CoordinateSpace coordinateSpace = CoordinateSpace.LocalToMonoPathAnimation;
    public PathType pathType = PathType.Linear;
    public Ease Ease = Ease.Linear;

    [Header("Bounds Support")]
    public bool useBoundsSupport = false;
    public Vector3 boundsSize = Vector3.one;
    public Collider[] boundsOverlapCheckColliders = Array.Empty<Collider>();

    [SerializeField] private bool playReverse = false; // Флаг для обратной анимации

    public override float Duration => _duration;

    private Vector3[] GetPathPoints(bool reverse, Transform context)
    {
        if (Points == null || Points.Length == 0)
            throw new InvalidOperationException("Points array is null or empty");

        Vector3[] pathPoints;

        if (coordinateSpace == CoordinateSpace.LocalToMonoPathAnimation)
        {
            pathPoints = new Vector3[Points.Length];
            for (int i = 0; i < Points.Length; i++)
                pathPoints[i] = this.transform.TransformPoint(Points[i]); // Локальные -> мировые с привязкой к этому объекту
        }
        else // World
        {
            pathPoints = (Vector3[])Points.Clone(); // Точки уже в мировых координатах
        }

        if (reverse)
            Array.Reverse(pathPoints);

        return pathPoints;
    }

    public async UniTask PlayPath(Transform context, bool reverse, CancellationToken token)
    {
        if (context == null)
            throw new InvalidOperationException("Transform to animate is null");

        Vector3[] pathPoints = GetPathPoints(reverse, context);

        Tween pathTween = context.DOPath(pathPoints, Duration, pathType)
            .SetOptions(false) // false — путь в мировых координатах
            .SetEase(Ease);

        await pathTween.ToUniTask(cancellationToken: token);
    }

    public override async UniTask Run(Transform context, CancellationToken token = default)
    {
        await PlayPath(context, playReverse, token);
    }

    public void SetDuration(float duration)
    {
        _duration = Mathf.Max(0f, duration);
    }

    [Button]
    public void RunAnimationButton() => Run(transformToAnimate).Forget();

    [Button]
    public void ToggleReverseAndRun()
    {
        playReverse = !playReverse;
        Run(transformToAnimate).Forget();
    }
}
