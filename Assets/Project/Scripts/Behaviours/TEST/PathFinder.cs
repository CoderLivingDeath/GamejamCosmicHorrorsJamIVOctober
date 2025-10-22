using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    // Находит ближайшую ноду из списка к точке
    public static MonoAIPathNode FindClosestNode(List<MonoAIPathNode> nodes, Vector3 point)
    {
        MonoAIPathNode closest = null;
        float minDist = float.MaxValue;
        foreach (var node in nodes)
        {
            if (node == null) continue;
            float dist = Vector3.SqrMagnitude(node.transform.position - point);
            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }
        }
        return closest;
    }

    // Поиск кратчайшего пути между двумя точками на графе (BFS)
    public static List<MonoAIPathNode> FindShortestPath(List<MonoAIPathNode> nodes, Vector3 startPos, Vector3 targetPos)
    {
        MonoAIPathNode startNode = FindClosestNode(nodes, startPos);
        MonoAIPathNode targetNode = FindClosestNode(nodes, targetPos);

        // Debug.Log($"Start node: {startNode?.name}, Target node: {targetNode?.name}");

        if (startNode == null || targetNode == null)
        {
            // Debug.LogWarning("Start or target node not found");
            return null;
        }
        if (startNode == targetNode)
            return new List<MonoAIPathNode> { startNode };

        Queue<MonoAIPathNode> queue = new Queue<MonoAIPathNode>();
        Dictionary<MonoAIPathNode, MonoAIPathNode> cameFrom = new Dictionary<MonoAIPathNode, MonoAIPathNode>();
        queue.Enqueue(startNode);
        cameFrom[startNode] = null;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == targetNode)
            {
                var path = new List<MonoAIPathNode>();
                MonoAIPathNode node = current;
                while (node != null)
                {
                    path.Add(node);
                    cameFrom.TryGetValue(node, out node);
                }
                path.Reverse();
                // Debug.Log($"Path found with {path.Count} nodes");
                return path;
            }

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor != null && !cameFrom.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    cameFrom[neighbor] = current;
                }
            }
        }
        // Debug.LogWarning("Path not found");
        return null;
    }

    // Возвращает соседей (NextNodes и PrevNodes)
    private static IEnumerable<MonoAIPathNode> GetNeighbors(MonoAIPathNode node)
    {
        foreach (var next in node.NextNodes)
            yield return next;
        foreach (var prev in node.PrevNodes)
            yield return prev;
    }

    // Поиск нод в радиусе от заданной точки
    public static List<MonoAIPathNode> FindNodesInRadius(List<MonoAIPathNode> nodes, Vector3 center, float radius)
    {
        List<MonoAIPathNode> result = new List<MonoAIPathNode>();
        float sqrRadius = radius * radius;

        foreach (var node in nodes)
        {
            if (node == null) continue;
            if ((node.transform.position - center).sqrMagnitude <= sqrRadius)
                result.Add(node);
        }
        return result;
    }

    // Поиск нод внутри Bounds
    public static List<MonoAIPathNode> FindNodesInBounds(List<MonoAIPathNode> nodes, Bounds bounds)
    {
        List<MonoAIPathNode> result = new List<MonoAIPathNode>();
        foreach (var node in nodes)
        {
            if (node == null) continue;
            if (bounds.Contains(node.transform.position))
                result.Add(node);
        }
        return result;
    }
}
