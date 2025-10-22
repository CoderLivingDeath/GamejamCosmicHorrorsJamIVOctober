using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(MonoAIPathNode), true)]
public class MonoAIPathNodeEditor : Editor
{
    private MonoAIPathNode node => (MonoAIPathNode)target;

    private SerializedProperty nextNodesProp;
    private SerializedProperty prevNodesProp;

    private void OnEnable()
    {
        nextNodesProp = serializedObject.FindProperty("NextNodes");
        prevNodesProp = serializedObject.FindProperty("PrevNodes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(nextNodesProp, new GUIContent("Next Nodes"), true);
        EditorGUILayout.PropertyField(prevNodesProp, new GUIContent("Prev Nodes"), true);

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

        serializedObject.ApplyModifiedProperties();
    }

    private void AddNewNode(Type nodeType)
    {
        // Находим MonoAIPath в иерархии выше
        MonoAIPath pathParent = node.GetComponentInParent<MonoAIPath>();
        if (pathParent == null)
        {
            Debug.LogWarning("MonoAIPath not found in parent hierarchy. Cannot create new node.");
            return;
        }

        GameObject newObj = new GameObject("PathNode_" + DateTime.Now.Ticks);
        newObj.transform.parent = pathParent.transform;  // Родитель - MonoAIPath, не сам узел
        newObj.transform.position = node.transform.position + Vector3.right * 1.5f;

        MonoAIPathNode newNode = (MonoAIPathNode)newObj.AddComponent(nodeType);

        // Добавляем новый узел в список MonoAIPath (если такая коллекция есть)
        if (!pathParent.Nodes.Contains(newNode))
        {
            pathParent.Nodes.Add(newNode);
            EditorUtility.SetDirty(pathParent);
        }

        // Связываем новый и текущий узлы
        if (!node.NextNodes.Contains(newNode))
            node.NextNodes.Add(newNode);
        if (!newNode.PrevNodes.Contains(node))
            newNode.PrevNodes.Add(node);

        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(node);
        EditorSceneManager.MarkSceneDirty(pathParent.gameObject.scene);
        Selection.activeGameObject = newObj;
    }
}
