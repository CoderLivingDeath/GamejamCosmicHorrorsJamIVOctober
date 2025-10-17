using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(MonoLocation))]
public class MonoLocationEditor : Editor
{
    private Color selectedBoundsColor = new Color(0, 1, 0, 0.3f);
    private Color otherBoundsColor = new Color(1, 0, 0, 0.2f);
    private Color selectedAnchorColor = Color.green;
    private Color otherAnchorColor = Color.blue;

    private const float snapThreshold = 1f;

    private struct AnchorCacheEntry
    {
        public MonoLocation sourceObject;
        public Vector3 worldAnchor;
    }

    private List<AnchorCacheEntry> otherAnchorsCache = new List<AnchorCacheEntry>();
    private MonoLocation selectedObject;
    private Vector3 lastPosition;

    private void OnEnable()
    {
        selectedObject = (MonoLocation)target;
        lastPosition = selectedObject.transform.position;
        CacheOtherAnchors();
        EditorApplication.update += OnEditorUpdate;
        RecalculateAndApplyBounds();
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (selectedObject == null) return;

        if (selectedObject.transform.position != lastPosition)
        {
            SnapToClosestAnchor();
            lastPosition = selectedObject.transform.position;
            RecalculateAndApplyBounds();
            SceneView.RepaintAll();
        }
    }

    private void CacheOtherAnchors()
    {
        otherAnchorsCache.Clear();
        var others = UnityEngine.Object.FindObjectsByType<MonoLocation>(UnityEngine.FindObjectsSortMode.None)
                    .Where(m => m != selectedObject && m.anchors != null);

        foreach (var other in others)
        {
            foreach (var anchor in other.anchors)
            {
                otherAnchorsCache.Add(new AnchorCacheEntry
                {
                    sourceObject = other,
                    worldAnchor = other.transform.TransformPoint(anchor)
                });
            }
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Refresh Anchors Cache"))
        {
            CacheOtherAnchors();
        }
    }

    private Bounds CalculateBounds()
    {
        var renderers = selectedObject.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(selectedObject.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private void RecalculateAndApplyBounds()
    {
        Bounds bounds = CalculateBounds();
        Undo.RecordObject(selectedObject, "Recalculate Bounds");
        selectedObject.bounds = bounds;
        EditorUtility.SetDirty(selectedObject);
    }

    private void DrawBounds(Bounds bounds, Color color, bool dashed = false)
    {
        Handles.color = color;

        Vector3 c = bounds.center;
        Vector3 ext = bounds.extents;

        Vector3[] corners = new Vector3[8]
        {
            c + new Vector3(-ext.x, -ext.y, -ext.z),
            c + new Vector3(ext.x, -ext.y, -ext.z),
            c + new Vector3(ext.x, -ext.y, ext.z),
            c + new Vector3(-ext.x, -ext.y, ext.z),

            c + new Vector3(-ext.x, ext.y, -ext.z),
            c + new Vector3(ext.x, ext.y, -ext.z),
            c + new Vector3(ext.x, ext.y, ext.z),
            c + new Vector3(-ext.x, ext.y, ext.z)
        };

        float lineWidth = 3f;

        if (dashed)
        {
            Handles.DrawDottedLine(corners[0], corners[1], 5f);
            Handles.DrawDottedLine(corners[1], corners[2], 5f);
            Handles.DrawDottedLine(corners[2], corners[3], 5f);
            Handles.DrawDottedLine(corners[3], corners[0], 5f);

            Handles.DrawDottedLine(corners[4], corners[5], 5f);
            Handles.DrawDottedLine(corners[5], corners[6], 5f);
            Handles.DrawDottedLine(corners[6], corners[7], 5f);
            Handles.DrawDottedLine(corners[7], corners[4], 5f);

            Handles.DrawDottedLine(corners[0], corners[4], 5f);
            Handles.DrawDottedLine(corners[1], corners[5], 5f);
            Handles.DrawDottedLine(corners[2], corners[6], 5f);
            Handles.DrawDottedLine(corners[3], corners[7], 5f);
        }
        else
        {
            for (int i = 0; i < 4; i++)
                Handles.DrawAAPolyLine(lineWidth, corners[i], corners[(i + 1) % 4]);
            for (int i = 4; i < 8; i++)
                Handles.DrawAAPolyLine(lineWidth, corners[i], corners[(i + 1) % 4 + 4]);

            Handles.DrawAAPolyLine(lineWidth, corners[0], corners[4]);
            Handles.DrawAAPolyLine(lineWidth, corners[1], corners[5]);
            Handles.DrawAAPolyLine(lineWidth, corners[2], corners[6]);
            Handles.DrawAAPolyLine(lineWidth, corners[3], corners[7]);
        }
    }

    private void DrawAnchors(MonoLocation ml, Vector3[] anchors, Color color)
    {
        Handles.color = color;
        if (anchors == null) return;

        Camera sceneCamera = SceneView.lastActiveSceneView.camera;
        if (sceneCamera == null) return;

        float baseHandleSize = 15f;

        for (int i = 0; i < anchors.Length; i++)
        {
            Vector3 worldPos = ml.transform.TransformPoint(anchors[i]);

            float distance = Vector3.Distance(sceneCamera.transform.position, worldPos);
            float size = HandleUtility.GetHandleSize(worldPos) * baseHandleSize * 0.01f;

            Handles.SphereHandleCap(0, worldPos, Quaternion.identity, size, EventType.Repaint);

            Vector3 labelPos = worldPos + sceneCamera.transform.up * size * 1.5f;

            Handles.BeginGUI();
            Vector3 screenPos = HandleUtility.WorldToGUIPoint(labelPos);
            GUIStyle style = new GUIStyle
            {
                normal = { textColor = color },
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            Rect rect = new Rect(screenPos.x - 10, screenPos.y - 10, 20, 20);
            GUI.Label(rect, i.ToString(), style);
            Handles.EndGUI();
        }
    }

    private void SnapToClosestAnchor()
    {
        if (selectedObject == null || selectedObject.anchors == null)
            return;

        Vector3 position = selectedObject.transform.position;
        Vector3 bestDelta = Vector3.zero;
        float minDistance = float.MaxValue;
        bool snapped = false;

        foreach (var anchorLocal in selectedObject.anchors)
        {
            Vector3 anchorWorld = selectedObject.transform.TransformPoint(anchorLocal);

            foreach (var cachedAnchor in otherAnchorsCache)
            {
                float distance = Vector3.Distance(anchorWorld, cachedAnchor.worldAnchor);

                if (distance < minDistance && distance <= snapThreshold)
                {
                    minDistance = distance;
                    bestDelta = cachedAnchor.worldAnchor - anchorWorld;
                    snapped = true;
                }
            }
        }

        if (snapped)
        {
            Undo.RecordObject(selectedObject.transform, "Snap MonoLocation To Anchor");
            selectedObject.transform.position += bestDelta;
            EditorUtility.SetDirty(selectedObject.transform);
        }
    }

    void OnSceneGUI()
    {
        if (selectedObject == null) return;

        if (otherAnchorsCache.Count == 0)
            CacheOtherAnchors();

        DrawBounds(selectedObject.bounds, selectedBoundsColor, false);
        DrawAnchors(selectedObject, selectedObject.anchors, selectedAnchorColor);

        var others = UnityEngine.Object.FindObjectsByType<MonoLocation>(UnityEngine.FindObjectsSortMode.None)
                    .Where(m => m != selectedObject);

        foreach (var other in others)
        {
            Bounds otherBounds = CalculateBoundsOther(other);
            DrawBounds(otherBounds, otherBoundsColor, true);
        }

        Handles.color = otherAnchorColor;
        foreach (var entry in otherAnchorsCache)
        {
            Handles.SphereHandleCap(0, entry.worldAnchor, Quaternion.identity, 0.1f, EventType.Repaint);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 newPosition = Handles.PositionHandle(selectedObject.transform.position, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(selectedObject.transform, "Move MonoLocation");
            selectedObject.transform.position = newPosition;
            SnapToClosestAnchor();
            lastPosition = selectedObject.transform.position;
            RecalculateAndApplyBounds();
            SceneView.RepaintAll();
        }
    }

    private Bounds CalculateBoundsOther(MonoLocation ml)
    {
        var renderers = ml.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(ml.transform.position, Vector3.zero);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }
}
