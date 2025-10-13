using UnityEngine;

public abstract class MonoCharacter : MonoBehaviour
{
    public Transform Transform => this.transform;
    public Bounds Bounds => Collider.bounds;
    public Collider Collider;

    // Хранит последний хит Raycast
    private bool hasLastHit = false;
    private RaycastHit lastHit;
    private Vector3 lastRayOrigin;

    public void SnapToSurface()
    {
        Vector3 boundsCenter = Bounds.center;
        float rayStartY = boundsCenter.y + Bounds.extents.y + 0.1f;
        Vector3 rayOrigin = new Vector3(boundsCenter.x, rayStartY, boundsCenter.z);

        Vector3 position = transform.position;

        int layerMask = ~(1 << LayerMask.NameToLayer("Character"));
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 5f, layerMask))
        {
            position.y = hit.point.y + Bounds.extents.y;

            // Сохраняем попадание для отрисовки
            lastHit = hit;
            lastRayOrigin = rayOrigin;
            hasLastHit = true;
        }
        else
        {
            hasLastHit = false;
        }

        Transform.position = position;
    }

    protected virtual void OnDrawGizmos()
    {
        if (Collider == null)
            return;

        Bounds b = Bounds;

        Vector3 center = b.center;
        Vector3 ext = b.extents;

        // Центр bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(center, 0.05f);

        // Вершины бокса (8 точек)
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

        Gizmos.color = Color.cyan;
        foreach (var corner in corners)
            Gizmos.DrawSphere(corner, 0.03f);

        // Центры граней (6 точек)
        Vector3[] faceCenters = new Vector3[6]
        {
            center + new Vector3(ext.x, 0, 0),
            center + new Vector3(-ext.x, 0, 0),
            center + new Vector3(0, ext.y, 0),
            center + new Vector3(0, -ext.y, 0),
            center + new Vector3(0, 0, ext.z),
            center + new Vector3(0, 0, -ext.z)
        };

        Gizmos.color = Color.magenta;
        foreach (var faceCenter in faceCenters)
            Gizmos.DrawSphere(faceCenter, 0.04f);

        // Рёбра бокса (линии)
        Gizmos.color = Color.green;

        void DrawEdge(int i1, int i2) => Gizmos.DrawLine(corners[i1], corners[i2]);

        DrawEdge(0, 4);
        DrawEdge(1, 5);
        DrawEdge(2, 6);
        DrawEdge(3, 7);

        DrawEdge(0, 1);
        DrawEdge(2, 3);
        DrawEdge(4, 5);
        DrawEdge(6, 7);

        DrawEdge(0, 2);
        DrawEdge(1, 3);
        DrawEdge(4, 6);
        DrawEdge(5, 7);

        // Отрисовка луча Raycast и точки попадания из SnapToSurface
        if (hasLastHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(lastRayOrigin, lastHit.point);
            Gizmos.DrawSphere(lastHit.point, 0.06f);
        }
    }
}
