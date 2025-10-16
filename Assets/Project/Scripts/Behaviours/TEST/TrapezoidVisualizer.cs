using UnityEngine;

/// <summary>
/// Класс для генерации меша трапеции и автоматического применения его в MeshCollider.
/// Включает валидацию: MeshCollider должен быть convex и isTrigger.
/// </summary>
[RequireComponent(typeof(MeshCollider))]
[ExecuteAlways]
public class TrapezoidMeshCollider : MonoBehaviour
{
    [Tooltip("Ширина нижнего основания по X")]
    public float bottomWidth = 2f;

    [Tooltip("Ширина верхнего основания по X")]
    public float topWidth = 1f;

    [Tooltip("Глубина трапеции по Z")]
    public float depth = 1f;

    [Tooltip("Высота трапеции по Y")]
    public float height = 3f;

    private MeshCollider meshCollider;
    private Mesh mesh;

    private void OnEnable()
    {
        meshCollider = GetComponent<MeshCollider>();
        ValidateColliderSettings();
        GenerateAndApplyMesh();
    }

    private void OnValidate()
    {
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();

        ValidateColliderSettings();
        GenerateAndApplyMesh();
    }

    private void ValidateColliderSettings()
    {
        if (meshCollider == null)
            return;

        // Убедиться, что MeshCollider является выпуклым и триггером
        if (!meshCollider.convex)
        {
            Debug.LogWarning($"{nameof(MeshCollider)} на объекте '{gameObject.name}' должен быть Convex. Значение будет автоматически установлено.");
            meshCollider.convex = true;
        }

        if (!meshCollider.isTrigger)
        {
            Debug.LogWarning($"{nameof(MeshCollider)} на объекте '{gameObject.name}' должен быть Trigger. Значение будет автоматически установлено.");
            meshCollider.isTrigger = true;
        }
    }

    private void GenerateAndApplyMesh()
    {
        mesh = new Mesh();
        mesh.name = "TrapezoidMesh";

        Vector3 bl = new Vector3(-bottomWidth / 2f, 0, -depth / 2f);
        Vector3 br = new Vector3(bottomWidth / 2f, 0, -depth / 2f);
        Vector3 brf = new Vector3(bottomWidth / 2f, 0, depth / 2f);
        Vector3 blf = new Vector3(-bottomWidth / 2f, 0, depth / 2f);

        Vector3 tl = new Vector3(-topWidth / 2f, height, -depth / 2f);
        Vector3 tr = new Vector3(topWidth / 2f, height, -depth / 2f);
        Vector3 trf = new Vector3(topWidth / 2f, height, depth / 2f);
        Vector3 tlf = new Vector3(-topWidth / 2f, height, depth / 2f);

        Vector3[] vertices = new Vector3[]
        {
      bl, br, brf, blf,
      tl, tr, trf, tlf
        };

        int[] triangles = new int[]
        {
      0, 1, 2, 0, 2, 3,          // Нижнее основание
                  4, 7, 6, 4, 6, 5,          // Верхнее основание
                  0, 3, 7, 0, 7, 4,          // Левая грань
                  1, 5, 6, 1, 6, 2,          // Правая грань
                  3, 2, 6, 3, 6, 7,          // Передняя грань
                  0, 4, 5, 0, 5, 1        // Задняя грань
            };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshCollider.sharedMesh = mesh;
    }
}
