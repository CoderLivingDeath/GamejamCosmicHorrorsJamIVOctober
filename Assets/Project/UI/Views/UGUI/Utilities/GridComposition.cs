using System;
using System.Collections.Generic;
using UnityEngine;

[Obsolete("Класс эксперементальный и может измениться.", true)]
[ExecuteAlways]
public class GridComposition : MonoBehaviour
{
    public string columnsContainerName = "Columns";
    public string rowsContainerName = "Rows";

    public Vector2 spacing = new Vector2(10, 10);
    public Vector2 padding = new Vector2(10, 10);

    public RectTransform RectTransform => (RectTransform)transform;

    void OnValidate() => ComposeGrid();
#if UNITY_EDITOR
    void Update() { if (!Application.isPlaying) ComposeGrid(); }
#endif

    void ComposeGrid()
    {
        var columnsGO = transform.Find(columnsContainerName);
        var rowsGO = transform.Find(rowsContainerName);
        if (columnsGO == null || rowsGO == null) return;

        var columns = new List<ColumnDefinition>(columnsGO.GetComponentsInChildren<ColumnDefinition>());
        var rows = new List<RowDefinition>(rowsGO.GetComponentsInChildren<RowDefinition>());
        if (columns.Count == 0 || rows.Count == 0) return;

        Vector2 gridSize = RectTransform.rect.size;
        float totalSpacingX = spacing.x * (columns.Count - 1);
        float totalSpacingY = spacing.y * (rows.Count - 1);
        float totalPaddingX = padding.x * 2;
        float totalPaddingY = padding.y * 2;

        float contentWidth = gridSize.x - totalSpacingX - totalPaddingX;
        float contentHeight = gridSize.y - totalSpacingY - totalPaddingY;
        float totalColScale = 0f, totalRowScale = 0f;
        foreach (var col in columns) totalColScale += Mathf.Max(0.001f, col.scale);
        foreach (var row in rows) totalRowScale += Mathf.Max(0.001f, row.scale);

        float[] colWidths = new float[columns.Count];
        float[] rowHeights = new float[rows.Count];
        for (int i = 0; i < columns.Count; i++)
            colWidths[i] = contentWidth * (columns[i].scale / totalColScale);
        for (int i = 0; i < rows.Count; i++)
            rowHeights[i] = contentHeight * (rows[i].scale / totalRowScale);

        float[] colPos = new float[columns.Count];
        float[] rowPos = new float[rows.Count];

        // Задаём значение только первому элементу массива, а не всему массиву сразу!
        colPos[0] = padding.x;
        for (int i = 1; i < columns.Count; i++)
            colPos[i] = colPos[i - 1] + colWidths[i - 1] + spacing.x;

        rowPos[0] = -padding.y;
        for (int i = 1; i < rows.Count; i++)
            rowPos[i] = rowPos[i - 1] - rowHeights[i - 1] - spacing.y;

        List<RectTransform> children = new List<RectTransform>();
        foreach (Transform child in transform)
        {
            if (child.name == columnsContainerName || child.name == rowsContainerName)
                continue;
            var rt = child as RectTransform;
            if (rt != null)
                children.Add(rt);
        }

        int childIdx = 0;
        for (int r = 0; r < rows.Count; r++)
        {
            for (int c = 0; c < columns.Count; c++)
            {
                if (childIdx >= children.Count) return;
                var child = children[childIdx++];
                child.anchorMin = new Vector2(0, 1);
                child.anchorMax = new Vector2(0, 1);
                child.pivot = new Vector2(0, 1);
                child.anchoredPosition = new Vector2(colPos[c], rowPos[r]);
                child.sizeDelta = new Vector2(colWidths[c], rowHeights[r]);
            }
        }
    }
}

