// using System.Collections.Generic;
// using UnityEngine;

// public class AIPathFinding
// {
//     public class Node
//     {
//         public Vector3 Point;
//         public bool Walkable;
//         public bool InteractionRequired;
//         public List<Node> Neighbors = new List<Node>();

//         public int GCost = int.MaxValue, HCost;
//         public Node Parent;

//         public int FCost => GCost + HCost;

//         // Дополнительно ссылка на исходный MonoAIPathNode
//         public MonoAIPathNode Source;

//         public Node(MonoAIPathNode source, bool walkable, bool interactionRequired = false)
//         {
//             Source = source;
//             Point = source.transform.position;
//             Walkable = walkable;
//             InteractionRequired = interactionRequired;
//         }
//     }

//     private List<Node> nodes;

//     // Конструктор принимает список Unity-узлов и строит граф нод AIPathSearch
//     public AIPathFinding(List<MonoAIPathNode> monoNodes)
//     {
//         // Создаем Node-обертки
//         nodes = new List<Node>();
//         var lookup = new Dictionary<MonoAIPathNode, Node>();
//         foreach (var monoNode in monoNodes)
//         {
//             // Предположим, Walkable всегда true, InteractionRequired — если MonoAIPathNodeWithInteraction
//             bool interactionRequired = monoNode is MonoAIPathNodeWithInteraction;
//             var node = new Node(monoNode, true, interactionRequired);
//             nodes.Add(node);
//             lookup[monoNode] = node;
//         }

//         // Подключаем соседей по ссылкам Next и Prev из MonoAIPathNode
//         foreach (var node in nodes)
//         {
//             if (node.Source.Next != null && lookup.ContainsKey(node.Source.Next))
//                 node.Neighbors.Add(lookup[node.Source.Next]);
//             if (node.Source.Prev != null && lookup.ContainsKey(node.Source.Prev))
//                 node.Neighbors.Add(lookup[node.Source.Prev]);
//         }
//     }

//     // Далее идут методы IsWalkable, GetHeuristic, CanInteract и FindPath как у вас

//     private bool IsWalkable(Node node)
//     {
//         return node.Walkable;
//     }

//     private int GetHeuristic(Node a, Node b)
//     {
//         // Используем манхэттенское расстояние по X и Z координатам позиции
//         Vector3 diff = a.Point - b.Point;
//         return Mathf.Abs((int)diff.x) + Mathf.Abs((int)diff.z);
//     }

//     private bool CanInteract(Node from, Node to)
//     {
//         if (!to.InteractionRequired) return true;

//         var interactableNode = to.Source as MonoAIPathNodeWithInteraction;
//         if (interactableNode != null && interactableNode.interactable != null)
//         {
//             // Можно добавить вызов проверки и выполнения интеракции
//             // return interactableNode.interactable.CanInteract();
//             return true; // заглушка
//         }
//         return false;
//     }

//     public List<Node> FindPath(Node start, Node target)
//     {
//         var openSet = new List<Node> { start };
//         var closedSet = new HashSet<Node>();

//         start.GCost = 0;
//         start.HCost = GetHeuristic(start, target);
//         start.Parent = null;

//         while (openSet.Count > 0)
//         {
//             Node current = openSet[0];
//             foreach (var n in openSet)
//             {
//                 if (n.FCost < current.FCost || (n.FCost == current.FCost && n.HCost < current.HCost))
//                     current = n;
//             }

//             if (current == target)
//                 return RetracePath(start, target);

//             openSet.Remove(current);
//             closedSet.Add(current);

//             foreach (var neighbor in current.Neighbors)
//             {
//                 if (!IsWalkable(neighbor) || closedSet.Contains(neighbor))
//                     continue;

//                 if (neighbor.InteractionRequired && !CanInteract(current, neighbor))
//                     continue;

//                 int tentativeG = current.GCost + 1;
//                 if (tentativeG < neighbor.GCost || !openSet.Contains(neighbor))
//                 {
//                     neighbor.GCost = tentativeG;
//                     neighbor.HCost = GetHeuristic(neighbor, target);
//                     neighbor.Parent = current;

//                     if (!openSet.Contains(neighbor))
//                         openSet.Add(neighbor);
//                 }
//             }
//         }
//         return null;
//     }

//     private List<Node> RetracePath(Node start, Node end)
//     {
//         var path = new List<Node>();
//         var current = end;
//         while (current != start)
//         {
//             path.Add(current);
//             current = current.Parent;
//         }
//         path.Reverse();
//         return path;
//     }
// }
