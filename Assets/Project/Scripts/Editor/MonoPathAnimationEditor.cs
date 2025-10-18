using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonoPathAnimation))]
public class MonoPathAnimationEditor : Editor
{
    private MonoPathAnimation script;
    private MonoPathAnimationEditorSettings settings;

    private void OnEnable()
    {
        script = (MonoPathAnimation)target;
        if (script.Points == null || script.Points.Length == 0)
        {
            script.Points = new Vector3[2] { Vector3.zero, Vector3.forward };
        }

        // Load settings asset from Resources folder with exact name
        settings = Resources.Load<MonoPathAnimationEditorSettings>("MonoPathAnimationEditorSettings");
        if (settings == null)
        {
            Debug.LogWarning("MonoPathAnimationEditorSettings not found in Resources! Using default settings.");
            settings = ScriptableObject.CreateInstance<MonoPathAnimationEditorSettings>();
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Run Animation"))
        {
            script.RunAnimationButton();
        }

        if (GUILayout.Button("Add Point"))
        {
            Array.Resize(ref script.Points, script.Points.Length + 1);
            Vector3 lastPoint = script.Points[script.Points.Length - 2];
            Vector3 newPoint = lastPoint + Vector3.forward;
            script.Points[script.Points.Length - 1] = newPoint;
            EditorUtility.SetDirty(script);
        }

        if (GUILayout.Button("Remove Last Point"))
        {
            if (script.Points.Length > 2)
            {
                Array.Resize(ref script.Points, script.Points.Length - 1);
                EditorUtility.SetDirty(script);
            }
        }

        if (GUILayout.Button("Snap All Points To Surface"))
        {
            SnapAllPointsToSurface();
        }
    }
    private void OnSceneGUI()
    {
        if (script.Points == null)
            return;

        Transform handleTransform = script.transform;
        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
            handleTransform.rotation : Quaternion.identity;

        for (int i = 0; i < script.Points.Length; i++)
        {
            Vector3 currentWorldPos = script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation
                ? handleTransform.TransformPoint(script.Points[i])
                : script.Points[i];

            // Корректируем центр bounds по флагу useBoundsBottomPivot
            Vector3 boundsCenter = currentWorldPos;
            if (script.useBoundsBottomPivot)
            {
                boundsCenter = currentWorldPos - Vector3.down * (script.boundsSize.y * 0.5f);
            }

            bool fitsBounds = true;
            if (script.useBoundsSupport)
            {
                fitsBounds = CanPlaceBoundsAtPosition(boundsCenter, script.boundsSize, script.boundsOverlapCheckColliders, script.BoundsCollisionLayers);
            }

            Color cubeColor;
            if (i == 0)
            {
                cubeColor = fitsBounds ? settings.boundsColorFirstPointActive : settings.boundsColorBad;
            }
            else if (i == script.Points.Length - 1)
            {
                cubeColor = fitsBounds ? settings.boundsColorLastPointActive : settings.boundsColorBad;
            }
            else
            {
                cubeColor = fitsBounds ? settings.boundsColorActive : settings.boundsColorInactive;
            }

            Handles.color = cubeColor;
            Handles.DrawWireCube(boundsCenter, script.boundsSize);

            DrawCapsule(boundsCenter, script.boundsSize, cubeColor);

            EditorGUI.BeginChangeCheck();

            Vector3 newWorldPos = Handles.PositionHandle(currentWorldPos, handleRotation);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(script, $"Move Point {i}");

                if (script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation)
                {
                    script.Points[i] = handleTransform.InverseTransformPoint(newWorldPos);
                }
                else
                {
                    script.Points[i] = newWorldPos;
                }

                EditorUtility.SetDirty(script);
            }

            float handleSize = HandleUtility.GetHandleSize(newWorldPos);

            Vector3 labelPos = newWorldPos + Vector3.Scale(settings.labelPositionOffset, Vector3.one * handleSize);
            Vector3 buttonPos = newWorldPos + Vector3.Scale(settings.buttonPositionOffset, Vector3.one * handleSize);

            Handles.BeginGUI();

            Vector2 labelGuiPos = HandleUtility.WorldToGUIPoint(labelPos);
            Vector2 buttonGuiPos = HandleUtility.WorldToGUIPoint(buttonPos);

            Rect labelRect = new Rect(labelGuiPos.x - settings.labelWidth / 2, labelGuiPos.y - 10, settings.labelWidth, 20);
            Rect buttonRect = new Rect(buttonGuiPos.x - 25, buttonGuiPos.y - 12, 50, 24);

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = settings.labelColor }
            };

            GUI.Label(labelRect, $"P{i}: {script.Points[i].ToString("F2")}", style);

            if (GUI.Button(buttonRect, "Snap"))
            {
                SnapPointToSurface(i, handleTransform);
            }

            Handles.EndGUI();
        }

        Vector3[] pointsWorld = GetPointsWorldPositions();

        Handles.color = settings.edgeColor;
        Handles.DrawAAPolyLine(settings.lineThickness, pointsWorld);
    }

    /// <summary>
    /// Рисует капсулу вписанную в bounds.
    /// Ориентация вдоль оси Y.
    /// </summary>
    private void DrawCapsuleInBounds(Vector3 center, Vector3 size, Color color)
    {
        Handles.color = color;

        float height = size.y;
        float radius = Mathf.Min(size.x, size.z) * 0.5f;

        // Центр цилиндра без половинок капсулы
        float cylinderHeight = height - 2 * radius;
        if (cylinderHeight < 0)
        {
            // Высота слишком мала, капсула – сфера
            cylinderHeight = 0;
            radius = height * 0.5f;
        }

        Vector3 up = Vector3.up;

        Vector3 start = center + up * (cylinderHeight * 0.5f);
        Vector3 end = center - up * (cylinderHeight * 0.5f);

        // Рисуем капсулу
        Handles.DrawWireDisc(start, up, radius);
        Handles.DrawWireDisc(end, up, radius);

        Handles.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
        Handles.DrawLine(start - Vector3.right * radius, end - Vector3.right * radius);
        Handles.DrawLine(start + Vector3.forward * radius, end + Vector3.forward * radius);
        Handles.DrawLine(start - Vector3.forward * radius, end - Vector3.forward * radius);
    }
    private void DrawCapsule(Vector3 center, Vector3 size, Color color)
    {
        Handles.color = color;

        float radius = Mathf.Min(size.x, size.z) * 0.5f;
        float height = size.y;

        // Высота цилиндрической части капсулы
        float cylinderHeight = Mathf.Max(0, height - 2 * radius);

        Vector3 up = Vector3.up;
        Vector3 centerTop = center + up * (cylinderHeight * 0.5f);
        Vector3 centerBottom = center - up * (cylinderHeight * 0.5f);

        // Верхняя полусфера - дуги направления «вверх»
        Handles.DrawWireArc(centerTop, Vector3.right, Vector3.back, 180, radius);
        Handles.DrawWireArc(centerTop, Vector3.forward, Vector3.right, 180, radius);

        // Нижняя полусфера - дуги направления «вниз»
        Handles.DrawWireArc(centerBottom, Vector3.right, Vector3.forward, 180, radius);
        Handles.DrawWireArc(centerBottom, Vector3.forward, Vector3.left, 180, radius);

        // Окружности основания цилиндра
        Handles.DrawWireDisc(centerTop, up, radius);
        Handles.DrawWireDisc(centerBottom, up, radius);

        // Боковые линии
        Vector3[] sideOffsets = new Vector3[]
        {
        Vector3.forward * radius,
        -Vector3.forward * radius,
        Vector3.right * radius,
        -Vector3.right * radius
        };

        foreach (var offset in sideOffsets)
        {
            Handles.DrawLine(centerTop + offset, centerBottom + offset);
        }
    }


    private Vector3[] GetPointsWorldPositions()
    {
        Transform handleTransform = script.transform;
        Vector3[] pointsWorld = new Vector3[script.Points.Length];
        for (int i = 0; i < script.Points.Length; i++)
        {
            pointsWorld[i] = script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation
                ? handleTransform.TransformPoint(script.Points[i])
                : script.Points[i];
        }

        return pointsWorld;
    }
    private void DrawBoundsWithSettings(Vector3 center, Vector3 size, Color color, float lineThickness, float sphereRadius)
    {
        Handles.color = color;

        Vector3 ext = size * 0.5f;

        Vector3[] corners = new Vector3[8]
        {
        center + new Vector3(+ext.x, +ext.y, +ext.z),
        center + new Vector3(+ext.x, +ext.y, -ext.z),
        center + new Vector3(+ext.x, -ext.y, +ext.z),
        center + new Vector3(+ext.x, -ext.y, -ext.z),
        center + new Vector3(-ext.x, +ext.y, +ext.z),
        center + new Vector3(-ext.x, +ext.y, -ext.z),
        center + new Vector3(-ext.x, -ext.y, +ext.z),
        center + new Vector3(-ext.x, -ext.y, -ext.z)
        };

        int[,] edges = new int[,]
        {
        {0,1}, {0,2}, {0,4},
        {1,3}, {1,5},
        {2,3}, {2,6},
        {3,7},
        {4,5}, {4,6},
        {5,7},
        {6,7}
        };

        for (int e = 0; e < 12; e++)
        {
            Handles.DrawAAPolyLine(lineThickness, new Vector3[] { corners[edges[e, 0]], corners[edges[e, 1]] });
        }

        foreach (var corner in corners)
        {
            Handles.SphereHandleCap(0, corner, Quaternion.identity, sphereRadius, EventType.Repaint);
        }
    }

    private bool CanPlaceBoundsAtPosition(Vector3 center, Vector3 size, Collider[] ignoreColliders, LayerMask supportedLayers)
    {
        Collider[] hits = Physics.OverlapBox(center, size * 0.5f, Quaternion.identity, supportedLayers);
        foreach (var hit in hits)
        {
            if (hit == null)
                continue;

            // Игнорируем коллайдеры из списка исключений
            bool isIgnored = false;
            if (ignoreColliders != null)
            {
                for (int i = 0; i < ignoreColliders.Length; i++)
                {
                    if (hit == ignoreColliders[i])
                    {
                        isIgnored = true;
                        break;
                    }
                }
            }

            if (!isIgnored)
            {
                return false; // Есть коллайдер вне игнор-листа на поддерживаемом слое
            }
        }
        return true;
    }


    private void SnapPointToSurface(int index, Transform handleTransform)
    {
        Vector3 origin = script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation
            ? handleTransform.TransformPoint(script.Points[index])
            : script.Points[index];

        int layerMask = script.BoundsCollisionLayers;

        float maxDistance = 10f;
        Vector3 rayOrigin = origin + Vector3.up * 5f;
        Vector3 rayDir = Vector3.down;

        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDir, maxDistance, layerMask);

        // Фильтруем все попадания, исключая игнорируемые коллайдеры
        RaycastHit? validHit = null;
        foreach (var hit in hits)
        {
            if (script.boundsOverlapCheckColliders != null &&
                Array.Exists(script.boundsOverlapCheckColliders, c => c == hit.collider))
            {
                continue; // Игнорируем этот хит
            }

            if (validHit == null || hit.distance < validHit.Value.distance)
            {
                validHit = hit; // Берём ближайший подходящий хит
            }
        }

        if (validHit.HasValue)
        {
            Vector3 snappedPos = validHit.Value.point;

            if (script.useBoundsSupport)
            {
                float heightOffset = 0.01f;
                snappedPos += Vector3.up * (script.boundsSize.y * 0.5f + heightOffset);
            }

            Undo.RecordObject(script, $"Snap Point {index} To Surface");

            if (script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation)
            {
                script.Points[index] = handleTransform.InverseTransformPoint(snappedPos);
            }
            else
            {
                script.Points[index] = snappedPos;
            }

            EditorUtility.SetDirty(script);
        }
    }

    private void SnapAllPointsToSurface()
    {
        Undo.RecordObject(script, "Snap All Points To Surface");

        Transform handleTransform = script.transform;
        int layerMask = ~0;

        float heightOffset = 0.01f;

        for (int i = 0; i < script.Points.Length; i++)
        {
            Vector3 worldPos = script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation
                ? handleTransform.TransformPoint(script.Points[i])
                : script.Points[i];

            Ray ray = new Ray(worldPos + Vector3.up * 5f, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, layerMask))
            {
                Vector3 snappedPos = hit.point;

                if (script.useBoundsSupport)
                {
                    snappedPos += Vector3.up * (script.boundsSize.y * 0.5f + heightOffset);
                }

                if (script.coordinateSpace == MonoPathAnimation.CoordinateSpace.LocalToMonoPathAnimation)
                {
                    script.Points[i] = handleTransform.InverseTransformPoint(snappedPos);
                }
                else
                {
                    script.Points[i] = snappedPos;
                }
            }
        }

        EditorUtility.SetDirty(script);
    }
}