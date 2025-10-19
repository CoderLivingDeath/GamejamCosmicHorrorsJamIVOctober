using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

[CustomEditor(typeof(InteractableBehaviour))]
public class InteractableBehaviourEditor : Editor
{
    private InteractableBehaviour targetBehaviour;
    private BoxBoundsHandle boundsHandle = new BoxBoundsHandle();

    private void OnEnable()
    {
        targetBehaviour = (InteractableBehaviour)target;
        boundsHandle.SetColor(Color.cyan);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (targetBehaviour.BoundsSupport)
        {
            EditorGUILayout.HelpBox("Bounds editing enabled. You can adjust bounds in Scene view.", MessageType.Info);
        }
    }

    private void OnSceneGUI()
    {
        if (!targetBehaviour.BoundsSupport)
            return;

        Transform t = targetBehaviour.transform;

        // Рисуем bounds текущего объекта с возможностью редактирования
        Bounds localBounds = targetBehaviour.InteractionBounds ?? new Bounds(Vector3.zero, Vector3.one);

        Vector3 worldCenter = t.TransformPoint(localBounds.center);
        Vector3 worldSize = Vector3.Scale(localBounds.size, t.lossyScale);

        boundsHandle.center = worldCenter;
        boundsHandle.size = worldSize;

        EditorGUI.BeginChangeCheck();
        boundsHandle.DrawHandle();
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(targetBehaviour, "Changed Interaction Bounds");

            Vector3 newLocalCenter = t.InverseTransformPoint(boundsHandle.center);
            Vector3 newLocalSize = new Vector3(
                boundsHandle.size.x / (t.lossyScale.x != 0f ? t.lossyScale.x : 1f),
                boundsHandle.size.y / (t.lossyScale.y != 0f ? t.lossyScale.y : 1f),
                boundsHandle.size.z / (t.lossyScale.z != 0f ? t.lossyScale.z : 1f)
            );

            Bounds newLocalBounds = new Bounds(newLocalCenter, newLocalSize);

            SerializedObject so = new SerializedObject(targetBehaviour);
            SerializedProperty boundsProp = so.FindProperty("_interactionBounds");
            boundsProp.boundsValue = newLocalBounds;
            so.ApplyModifiedProperties();
        }

        var allInteractables = Object.FindObjectsByType<InteractableBehaviour>(FindObjectsSortMode.None);

        Handles.color = Color.yellow; // цвет для других interactables
        foreach (var interactable in allInteractables)
        {
            if (interactable == targetBehaviour)
                continue; // пропускаем текущий объект (он уже отрисован и редактируется)

            if (!interactable.BoundsSupport)
                continue; // если нет поддержки bounds, пропускаем

            Bounds otherLocalBounds = interactable.InteractionBounds ?? new Bounds(Vector3.zero, Vector3.one);
            Transform otherT = interactable.transform;

            Vector3 otherWorldCenter = otherT.TransformPoint(otherLocalBounds.center);
            Vector3 otherWorldSize = Vector3.Scale(otherLocalBounds.size, otherT.lossyScale);

            // Рисуем wire cube вокруг bounds другого объекта
            Handles.DrawWireCube(otherWorldCenter, otherWorldSize);
        }

        Handles.color = Color.white; // сброс цвета
    }
}
