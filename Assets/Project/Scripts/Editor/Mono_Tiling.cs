// TilingToolEditor.cs (��������� � ����� Editor)
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TilingTool))]
public class TilingToolEditor : Editor
{
    private Vector2Int oldTiling;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TilingTool tilingTool = (TilingTool)target;

        // ��������� ������ �������� ��� ���������
        Vector2Int currentTiling = tilingTool.Tiling;

        //if (GUILayout.Button("Generate Tiling"))
        //{
        //    GenerateTiling(tilingTool);
        //}

        // ���� �������� �������� ���������� � ���������� ������� �������������
        if (tilingTool.Tiling != currentTiling || tilingTool.Tiling != oldTiling)
        {
            GenerateTiling(tilingTool);
            oldTiling = tilingTool.Tiling;
        }
    }

    private void GenerateTiling(TilingTool tool)
    {
        if (tool.Target == null)
        {
            Debug.LogWarning("Target is not assigned");
            return;
        }

        // ������� ���� ����� �������� �������, ����� �������� ������ ����� ��������
        int childCount = tool.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(tool.transform.GetChild(i).gameObject);
        }

        // �������� ������ Target ����� Renderer.bounds ��� Collider.bounds
        Vector3 size = Vector3.one;
        Renderer renderer = tool.Target.GetComponent<Renderer>();
        if (renderer != null)
        {
            // ���������� sharedMaterial ���� ����� ���������, �� ��� �������� ������ ������� bounds � ��� ���������
            size = renderer.bounds.size;
        }
        else
        {
            Collider collider = tool.Target.GetComponent<Collider>();
            if (collider != null)
            {
                size = collider.bounds.size;
            }
            else
            {
                Debug.LogWarning("Target does not have Renderer or Collider to determine size, using (1,1,1)");
            }
        }

        float totalSizeX = size.x * tool.Tiling.x;
        float startX = -totalSizeX / 2 + size.x / 2;

        float totalSizeZ = size.z * tool.Tiling.y;
        float startZ = -totalSizeZ / 2 + size.z / 2;

        // ������� ������� �������� ��������
        for (int x = 0; x < tool.Tiling.x; x++)
        {
            for (int y = 0; y < tool.Tiling.y; y++)
            {
                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(tool.Target);
                obj.transform.parent = tool.transform;
                obj.transform.localPosition = new Vector3(startX + x * size.x, 0, startZ + y * size.z);
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;
            }
        }
    }
}
