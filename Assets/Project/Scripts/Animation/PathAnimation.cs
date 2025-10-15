using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class PathAnimation : IScriptableAnimation<Transform>
{
    public enum CoordinateSpace
    {
        World,
        LocalToMonoPathAnimation
    }

    private float _duration = 1f;
    public Vector3[] Points;
    public CoordinateSpace coordinateSpace = CoordinateSpace.LocalToMonoPathAnimation;
    public PathType pathType = PathType.Linear;
    public Ease Ease = Ease.Linear;
    public bool playReverse = false;

    public float Duration
    {
        get => _duration;
        set => _duration = Math.Max(0f, value);
    }

    public PathAnimation(Vector3[] points)
    {
        Points = points;
    }
    public async UniTask Run(Transform context, CancellationToken token = default)
    {
        if (Points == null || Points.Length == 0)
            throw new InvalidOperationException("Points array is null or empty");

        if (context == null)
            throw new InvalidOperationException("Transform to animate is null");

        Vector3[] pathPoints = coordinateSpace == CoordinateSpace.LocalToMonoPathAnimation
            ? TransformPointsLocalToWorld(Points, context)
            : (Vector3[])Points.Clone();

        if (playReverse)
            Array.Reverse(pathPoints);

        Tween pathTween = context.DOPath(pathPoints, Duration, pathType)
                            .SetOptions(false)
                            .SetEase(Ease);

        await pathTween.ToUniTask(cancellationToken: token);
    }

    private static Vector3[] TransformPointsLocalToWorld(Vector3[] points, Transform reference)
    {
        var transformed = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            transformed[i] = reference.TransformPoint(points[i]);
        return transformed;
    }
}
