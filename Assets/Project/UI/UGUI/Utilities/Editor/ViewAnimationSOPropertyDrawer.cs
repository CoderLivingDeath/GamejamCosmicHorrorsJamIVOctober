using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(ViewAnimationSO<MonoCanvasView>), true)]
public class ViewAnimationSOPropertyDrawer : PropertyDrawer
{
    private const float FoldoutWidth = 15f;
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        if (foldouts.TryGetValue(property.propertyPath, out bool foldout) && foldout && property.objectReferenceValue != null)
        {
            SerializedObject so = new SerializedObject(property.objectReferenceValue);
            so.Update();

            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.name == "m_Script") continue;
                height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Задаем область foldout слева
        Rect foldoutRect = new Rect(position.x, position.y, FoldoutWidth, EditorGUIUtility.singleLineHeight);
        // Остаток строки для лейбла и ObjectField справа от foldout
        Rect labelRect = new Rect(position.x + FoldoutWidth, position.y, position.width - FoldoutWidth, EditorGUIUtility.singleLineHeight);

        // Рисуем foldout слева с меткой (нажимаемая стрелочка и текст)
        if (!foldouts.ContainsKey(property.propertyPath))
            foldouts[property.propertyPath] = false;
        foldouts[property.propertyPath] = EditorGUI.Foldout(foldoutRect, foldouts[property.propertyPath], label, true);

        // Рисуем ObjectField справа от foldout и метки
        EditorGUI.BeginChangeCheck();
        Rect objectFieldRect = new Rect(labelRect.x + EditorGUIUtility.labelWidth, labelRect.y,
                                       labelRect.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
        Object obj = EditorGUI.ObjectField(objectFieldRect, GUIContent.none, property.objectReferenceValue, fieldInfo.FieldType, false);

        if (EditorGUI.EndChangeCheck())
        {
            property.objectReferenceValue = obj;
            foldouts[property.propertyPath] = false; // закрыть foldout при смене объекта
        }
        if (foldouts[property.propertyPath] && property.objectReferenceValue != null)
        {
            EditorGUI.indentLevel++;

            SerializedObject so = new SerializedObject(property.objectReferenceValue);
            so.Update();

            float yOffset = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            // Вычисляем высоту всех вложенных свойств
            float totalHeight = 0f;

            SerializedProperty prop = so.GetIterator();
            bool enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.name == "m_Script") continue;

                totalHeight += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            // Рисуем прямоугольник HelpBox
            Rect helpBoxRect = new Rect(position.x, yOffset - EditorGUIUtility.standardVerticalSpacing / 2f, position.width, totalHeight + EditorGUIUtility.standardVerticalSpacing);
            EditorGUI.DrawRect(helpBoxRect, new Color(0.22f, 0.25f, 0.3f, 0.2f)); // светлый фон

            // Рисуем вложенные свойства внутри helpBoxRect с отступом
            float innerYOffset = yOffset;
            prop = so.GetIterator();
            enterChildren = true;
            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (prop.name == "m_Script") continue;

                float propHeight = EditorGUI.GetPropertyHeight(prop, true);
                Rect propRect = new Rect(position.x + 4, innerYOffset, position.width - 8, propHeight);
                EditorGUI.PropertyField(propRect, prop, true);
                innerYOffset += propHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            so.ApplyModifiedProperties();
            EditorGUI.indentLevel--;
        }


        EditorGUI.EndProperty();
    }
}
