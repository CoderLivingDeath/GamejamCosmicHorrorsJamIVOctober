using UnityEngine;
using System.Collections;

[ExecuteAlways]
public class MonoLocation : MonoBehaviour
{
    public string Id = "Default location name";
    public Bounds bounds;

    public Vector3[] anchors;

    public GameObject[] objectsInBounds;

    public LayerMask detectionLayer;

    [Tooltip("Интервал обновления обнаружения объектов в секундах")]
    public float detectionInterval = 0.5f;

    private Coroutine detectionCoroutine;

    private void OnEnable()
    {
        StartDetection();
    }

    private void OnDisable()
    {
        StopDetection();
    }

    private void StartDetection()
    {
        if (detectionCoroutine != null)
            StopCoroutine(detectionCoroutine);
        detectionCoroutine = StartCoroutine(DetectObjectsPeriodically());
    }

    private void StopDetection()
    {
        if (detectionCoroutine != null)
        {
            StopCoroutine(detectionCoroutine);
            detectionCoroutine = null;
        }
    }

    private IEnumerator DetectObjectsPeriodically()
    {
        while (true)
        {
            DetectObjectsInBounds();

            yield return new WaitForSeconds(detectionInterval);
        }
    }

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

    public void DetectObjectsInBounds()
    {
        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, detectionLayer);
        objectsInBounds = new GameObject[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            objectsInBounds[i] = colliders[i].gameObject;
        }
    }
}