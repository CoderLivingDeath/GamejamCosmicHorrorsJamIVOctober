using System.Collections.Generic;
using UnityEngine;

public class PathFinderTester : MonoBehaviour
{
    public MonoAIPath aiPath;

    public Vector3 startPoint;
    public Vector3 targetPoint;

    public Vector3 queryPoint;
    public float queryRadius = 5f;

    public Bounds queryBounds = new Bounds(Vector3.zero, Vector3.one * 10f);

    // Смещение для гизмосов
    public Vector3 gizmoOffset = Vector3.up * 0.5f;

    // Путь между start и target
    public List<MonoAIPathNode> lastPath = new List<MonoAIPathNode>();

    // Ноды вокруг queryPoint в радиусе
    public List<MonoAIPathNode> foundNodesInRadius = new List<MonoAIPathNode>();

    // Ноды внутри queryBounds
    public List<MonoAIPathNode> foundNodesInBounds = new List<MonoAIPathNode>();

    void Update()
    {
        if (aiPath == null || aiPath.Nodes == null || aiPath.Nodes.Count == 0)
            return;

        lastPath = PathFinder.FindShortestPath(aiPath.Nodes, startPoint, targetPoint);
        foundNodesInRadius = PathFinder.FindNodesInRadius(aiPath.Nodes, queryPoint, queryRadius);
        foundNodesInBounds = PathFinder.FindNodesInBounds(aiPath.Nodes, queryBounds);
    }

    private void OnDrawGizmos()
    {
        // Рисуем начальную и конечную точки
        Gizmos.color = Color.green;
        Gizmos.DrawCube(startPoint + gizmoOffset, Vector3.one * 0.6f);
        Gizmos.color = Color.red;
        Gizmos.DrawCube(targetPoint + gizmoOffset, Vector3.one * 0.6f);

        // Рисуем путь жёлтым цветом
        if (lastPath != null && lastPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < lastPath.Count - 1; i++)
            {
                var nodeA = lastPath[i];
                var nodeB = lastPath[i + 1];
                if (nodeA != null && nodeB != null)
                {
                    Vector3 posA = nodeA.transform.position + gizmoOffset;
                    Vector3 posB = nodeB.transform.position + gizmoOffset;
                    Gizmos.DrawCube(posA, Vector3.one * 0.4f);
                    Gizmos.DrawLine(posA, posB);
                }
            }
            if (lastPath[lastPath.Count - 1] != null)
            {
                Vector3 posLast = lastPath[lastPath.Count - 1].transform.position + gizmoOffset;
                Gizmos.DrawCube(posLast, Vector3.one * 0.4f);
            }
        }

        // Рисуем запросную точку и радиус
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(queryPoint + gizmoOffset, 0.3f);
        Gizmos.DrawWireSphere(queryPoint + gizmoOffset, queryRadius);

        // Рисуем найденные ноды в радиусе красными кубами
        if (foundNodesInRadius != null && foundNodesInRadius.Count > 0)
        {
            Gizmos.color = Color.red;
            foreach (var node in foundNodesInRadius)
            {
                if (node != null)
                    Gizmos.DrawCube(node.transform.position + gizmoOffset, Vector3.one * 0.4f);
            }
        }

        // Рисуем область Bounds прозрачным синим кубом
        Gizmos.color = new Color(0f, 0f, 1f, 0.2f);
        Gizmos.DrawCube(queryBounds.center + gizmoOffset, queryBounds.size);

        // Рисуем найденные ноды в области Bounds жёлтыми кубами
        if (foundNodesInBounds != null && foundNodesInBounds.Count > 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var node in foundNodesInBounds)
            {
                if (node != null)
                    Gizmos.DrawCube(node.transform.position + gizmoOffset, Vector3.one * 0.45f);
            }
        }
    }
}
