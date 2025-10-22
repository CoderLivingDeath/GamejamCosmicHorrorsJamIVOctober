using UnityEngine;
using System.Collections.Generic;

public class PlayerPredictor : MonoBehaviour
{
    public Transform playerTransform;

    public float recordInterval = 0.2f;
    private float timeSinceLastRecord = 0f;

    private Queue<Vector3> positionHistory = new Queue<Vector3>();
    public int historySize = 5;
    public float predictionTimeStep = 0.1f;
    public int predictionCount = 10;

    public float smoothFactor = 0.5f;

    private Vector3 smoothedPosition;
    private Vector3 smoothedVelocity;

    void Update()
    {
        timeSinceLastRecord += Time.deltaTime;
        if (timeSinceLastRecord >= recordInterval)
        {
            Vector3 currentPos = playerTransform.position;

            if (positionHistory.Count == 0)
            {
                smoothedPosition = currentPos;
                smoothedVelocity = Vector3.zero;
            }
            else
            {
                Vector3 lastPos = smoothedPosition;
                Vector3 currentVelocity = (currentPos - lastPos) / recordInterval;

                smoothedPosition = Vector3.Lerp(smoothedPosition, currentPos, smoothFactor);
                smoothedVelocity = Vector3.Lerp(smoothedVelocity, currentVelocity, smoothFactor);
            }

            // Обновляем очередь для визуализации (если нужна)
            if (positionHistory.Count >= historySize)
                positionHistory.Dequeue();
            positionHistory.Enqueue(currentPos);

            timeSinceLastRecord = 0f;
        }
    }

    // Предсказываем позиции, продолжая движение со сглаженной скоростью
    public List<Vector3> GetPredictedPositions()
    {
        List<Vector3> predictions = new List<Vector3>();
        Vector3 lastPos = smoothedPosition;

        for (int i = 1; i <= predictionCount; i++)
        {
            float t = predictionTimeStep * i;
            // Экспоненциальная формула: прогноз по позиции + экспоненциальная скорость (можно изменять)
            Vector3 predicted = smoothedPosition + smoothedVelocity * t;
            predictions.Add(predicted);
            lastPos = predicted;
        }
        return predictions;
    }

    void OnDrawGizmos()
    {
        if (positionHistory == null || positionHistory.Count == 0)
            return;

        Gizmos.color = Color.blue;
        Vector3 prevPoint = Vector3.zero;
        bool first = true;
        foreach (var pos in positionHistory)
        {
            Gizmos.DrawSphere(pos, 0.2f);
            if (!first)
                Gizmos.DrawLine(prevPoint, pos);
            else
                first = false;
            prevPoint = pos;
        }

        var predictedPositions = GetPredictedPositions();
        Gizmos.color = Color.red;

        Vector3 lastPred = smoothedPosition;
        foreach (var p in predictedPositions)
        {
            Gizmos.DrawCube(p, Vector3.one * 0.25f);
            Gizmos.DrawLine(lastPred, p);
            lastPred = p;
        }
    }
}
