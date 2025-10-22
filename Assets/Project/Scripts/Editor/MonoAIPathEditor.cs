using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MonoAIPath), true)]
public class MonoAIPathEditor : Editor
{
    private MonoAIPath path => (MonoAIPath)target;

    private void OnEnable()
    {
        if (path.Nodes == null)
            path.Nodes = new List<MonoAIPathNode>();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Add New Node", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Simple Node"))
        {
            AddNewNode(typeof(MonoAIPathNode));
        }
        if (GUILayout.Button("Add Node With Interaction"))
        {
            AddNewNode(typeof(MonoAIPathNodeWithInteraction));
        }
    }

    private void AddNewNode(System.Type nodeType)
    {
        GameObject newNodeObj = new GameObject("PathNode_" + (path.Nodes.Count + 1));
        newNodeObj.transform.parent = path.transform;

        Vector3 pos = path.transform.position;
        if (path.Nodes.Count > 0)
            pos = path.Nodes[path.Nodes.Count - 1].transform.position + Vector3.right * 1.5f;
        newNodeObj.transform.position = pos;

        MonoAIPathNode newNode = (MonoAIPathNode)newNodeObj.AddComponent(nodeType);

        path.Nodes.Add(newNode);

        // В старой версии у нас был один Next/Prev, теперь множественные связи:
        // Связываем нового узла со старым как соседа NextNodes и PrevNodes
        if (path.Nodes.Count > 1)
        {
            var prevNode = path.Nodes[path.Nodes.Count - 2];
            if (!prevNode.NextNodes.Contains(newNode))
                prevNode.NextNodes.Add(newNode);
            if (!newNode.PrevNodes.Contains(prevNode))
                newNode.PrevNodes.Add(prevNode);
        }

        EditorUtility.SetDirty(path);
        EditorSceneManager.MarkSceneDirty(path.gameObject.scene);

        Selection.activeGameObject = newNodeObj;
    }

    // Отрисовка в сцене с учетом множества NextNodes
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Active)]
    static void DrawPathGizmos(MonoAIPath path, GizmoType gizmoType)
    {
        if (path.Nodes == null || path.Nodes.Count == 0)
            return;

        for (int i = 0; i < path.Nodes.Count; i++)
        {
            var node = path.Nodes[i];
            if (node == null) continue;

            Handles.color = (node is MonoAIPathNodeWithInteraction) ? Color.magenta : Color.green;
            Handles.SphereHandleCap(0, node.transform.position, Quaternion.identity, 0.25f, EventType.Repaint);
            Handles.Label(node.transform.position + Vector3.up * 0.3f, $"Node {i}");

            // Рисуем линии ко всем NextNodes
            if (node.NextNodes != null)
            {
                Handles.color = Color.yellow;
                foreach (var nextNode in node.NextNodes)
                {
                    if (nextNode != null)
                        Handles.DrawLine(node.transform.position, nextNode.transform.position);
                }
            }
        }
    }
}
