using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoSaveable))]
public class SaveableEditor : Editor
{
    private SerializedProperty idProp;
    private SerializedProperty componentParametersProp;

    private Dictionary<int, bool> foldoutStates = new Dictionary<int, bool>();
    private bool parametersListExpanded = true;

    private void OnEnable()
    {
        idProp = serializedObject.FindProperty("ID");
        componentParametersProp = serializedObject.FindProperty("ComponentParameters");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Рисуем поле ID с лейблом и кнопкой слева
        EditorGUILayout.BeginHorizontal();

        GUILayout.Label("ID", GUILayout.Width(EditorGUIUtility.labelWidth));

        GUIContent reloadIcon = EditorGUIUtility.IconContent("Refresh");
        reloadIcon.tooltip = "Generate new GUID";
        if (GUILayout.Button(reloadIcon, GUILayout.Width(24), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
        {
            MonoSaveable saveable = (MonoSaveable)target;
            Undo.RecordObject(saveable, "Generate new GUID");
            saveable.GenerateGUID();
            EditorUtility.SetDirty(saveable);
            serializedObject.Update();
        }

        EditorGUILayout.PropertyField(idProp, GUIContent.none);

        EditorGUILayout.EndHorizontal();

        // Отрисовка остальных полей, исключая ComponentParameters, ID и поле Script
        DrawDefaultInspectorExceptComponentParameters();

        // Отрисовка списка ComponentParameters с раскрывающимися элементами
        parametersListExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(parametersListExpanded, $"Component Parameters ({componentParametersProp.arraySize})");

        if (parametersListExpanded)
        {
            if (componentParametersProp.arraySize == 0)
            {
                if (GUILayout.Button("Add Component Parameter"))
                {
                    componentParametersProp.InsertArrayElementAtIndex(0);
                    var newElement = componentParametersProp.GetArrayElementAtIndex(0);
                    var targetCompProp = newElement.FindPropertyRelative("TargetComponent");
                    var paramNameProp = newElement.FindPropertyRelative("ParameterName");

                    targetCompProp.objectReferenceValue = null;
                    paramNameProp.stringValue = "";

                    serializedObject.ApplyModifiedProperties();
                }
            }
            else
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < componentParametersProp.arraySize; i++)
                {
                    var element = componentParametersProp.GetArrayElementAtIndex(i);

                    if (!foldoutStates.TryGetValue(i, out bool expanded))
                        expanded = true;

                    var targetComponentProp = element.FindPropertyRelative("TargetComponent");
                    var parameterNameProp = element.FindPropertyRelative("ParameterName");

                    string componentName = targetComponentProp.objectReferenceValue != null
                        ? targetComponentProp.objectReferenceValue.GetType().Name
                        : "null";

                    string parameterName = !string.IsNullOrEmpty(parameterNameProp.stringValue)
                        ? parameterNameProp.stringValue
                        : "null";

                    string foldoutLabel = $"{componentName} -> {parameterName}";

                    expanded = EditorGUILayout.Foldout(expanded, foldoutLabel, true);
                    foldoutStates[i] = expanded;

                    if (expanded)
                    {
                        EditorGUI.indentLevel++;
                        DrawComponentParameterContents(element, i);
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;

                if (GUILayout.Button("Add Component Parameter"))
                {
                    componentParametersProp.InsertArrayElementAtIndex(componentParametersProp.arraySize);
                    var newElement = componentParametersProp.GetArrayElementAtIndex(componentParametersProp.arraySize - 1);
                    var targetCompProp = newElement.FindPropertyRelative("TargetComponent");
                    var paramNameProp = newElement.FindPropertyRelative("ParameterName");

                    targetCompProp.objectReferenceValue = null;
                    paramNameProp.stringValue = "";

                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefaultInspectorExceptComponentParameters()
    {
        var iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        while (iterator.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (iterator.name == "ComponentParameters" || iterator.name == "ID" || iterator.name == "m_Script")
                continue;

            EditorGUILayout.PropertyField(iterator, true);
        }
    }
    private void DrawComponentParameterContents(SerializedProperty elementProp, int index)
    {
        SerializedProperty targetComponentProp = elementProp.FindPropertyRelative("TargetComponent");
        SerializedProperty parameterNameProp = elementProp.FindPropertyRelative("ParameterName");

        var saveable = (MonoSaveable)target;
        Component[] components = saveable.GetComponents<Component>();
        string[] componentNames = components.Select(c => c.GetType().Name).ToArray();

        int selectedIndex = -1;
        if (targetComponentProp.objectReferenceValue != null)
        {
            var currentType = targetComponentProp.objectReferenceValue.GetType();
            selectedIndex = Array.FindIndex(components, c => c.GetType() == currentType);
        }

        int newSelectedIndex = EditorGUILayout.Popup("Target Component", selectedIndex, componentNames);
        if (newSelectedIndex != selectedIndex)
        {
            targetComponentProp.objectReferenceValue = components[newSelectedIndex];
            parameterNameProp.stringValue = "";
        }

        Component selectedComp = targetComponentProp.objectReferenceValue as Component;
        if (selectedComp != null)
        {
            List<string> names = new List<string>();
            var type = selectedComp.GetType();

            names.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(f => !f.IsSpecialName && IsTypeSerializableIncludingVectors(f.FieldType))
                .Select(f => f.Name));
            names.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && p.CanWrite && IsTypeSerializableIncludingVectors(p.PropertyType))
                .Select(p => p.Name));
            names.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(f => Attribute.IsDefined(f, typeof(IncludeInSaveAttribute)) && IsTypeSerializableIncludingVectors(f.FieldType))
                .Select(f => f.Name));
            names.AddRange(type.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(p => p.CanRead && p.CanWrite && Attribute.IsDefined(p, typeof(IncludeInSaveAttribute)) && IsTypeSerializableIncludingVectors(p.PropertyType))
                .Select(p => p.Name));

            if (names.Count == 0)
            {
                EditorGUILayout.HelpBox("No public or included private fields/properties with get/set and serializable types.", MessageType.Info);
                if (!string.IsNullOrEmpty(parameterNameProp.stringValue))
                    parameterNameProp.stringValue = "";
            }
            else
            {
                // Запрет дубликатов параметров внутри одного компонента
                HashSet<string> usedParams = new HashSet<string>();
                for (int i = 0; i < componentParametersProp.arraySize; i++)
                {
                    if (i == index) continue;
                    var otherElement = componentParametersProp.GetArrayElementAtIndex(i);
                    var otherTargetProp = otherElement.FindPropertyRelative("TargetComponent");
                    var otherParamProp = otherElement.FindPropertyRelative("ParameterName");

                    if (otherTargetProp.objectReferenceValue == targetComponentProp.objectReferenceValue &&
                        !string.IsNullOrEmpty(otherParamProp.stringValue))
                    {
                        usedParams.Add(otherParamProp.stringValue);
                    }
                }

                var filteredNames = names.Where(n => !usedParams.Contains(n)).ToList();

                // Сброс значения, если параметр больше не валиден
                if (!string.IsNullOrEmpty(parameterNameProp.stringValue) && !filteredNames.Contains(parameterNameProp.stringValue))
                {
                    parameterNameProp.stringValue = "";
                }

                if (filteredNames.Count == 0)
                {
                    EditorGUILayout.HelpBox("No available parameters (all taken for this component).", MessageType.Warning);
                    parameterNameProp.stringValue = "";
                }
                else
                {
                    int paramIndex = 0;
                    if (!string.IsNullOrEmpty(parameterNameProp.stringValue))
                    {
                        paramIndex = filteredNames.IndexOf(parameterNameProp.stringValue);
                        if (paramIndex < 0) paramIndex = 0;
                    }

                    int newParamIndex = EditorGUILayout.Popup("Parameter Name", paramIndex, filteredNames.ToArray());
                    parameterNameProp.stringValue = filteredNames[newParamIndex];
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Select Target Component", MessageType.Warning);
            parameterNameProp.stringValue = "";
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Remove", GUILayout.Height(20)))
        {
            componentParametersProp.DeleteArrayElementAtIndex(index);
            return;
        }
    }

    // Вспомогательный метод для проверки сериализуемости с поддержкой Vector2, Vector3, Vector4
    private static bool IsTypeSerializableIncludingVectors(Type type)
    {
        if (Nullable.GetUnderlyingType(type) != null)
            type = Nullable.GetUnderlyingType(type);

        if (type.IsPrimitive || type.IsEnum || type == typeof(string))
            return true;

        if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            return true;

        if (Attribute.IsDefined(type, typeof(SerializableAttribute)))
            return true;

        // Добавляем поддержку Vector2, Vector3, Vector4
        if (type == typeof(Vector2) || type == typeof(Vector3) || type == typeof(Vector4))
            return true;

        return false;
    }

}
