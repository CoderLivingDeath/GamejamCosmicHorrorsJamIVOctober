using UnityEngine;

[ExecuteAlways]
public class MonoLocation : MonoBehaviour
{
    public string Id = "Default scene name";
    public Bounds bounds;

    public Vector3[] anchors;

    // Автоматический пересчет при любых изменениях (включая изменения в инспекторе)
    private void OnValidate()
    {
        RecalculateBounds();
    }

    // Пересчет в редакторе при любых изменениях трансформов
    private void Update()
    {
        if (!Application.isPlaying)
        {
            RecalculateBounds();
        }
    }

    [ContextMenu("Recalculate Bounds")]
    public void RecalculateBounds()
    {
        var renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            bounds = new Bounds(transform.position, Vector3.zero);
            return;
        }

        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
    }
}
