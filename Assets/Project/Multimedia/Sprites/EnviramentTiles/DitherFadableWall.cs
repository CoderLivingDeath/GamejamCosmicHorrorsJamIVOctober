using UnityEngine;

public class DitherFadableWall : MonoBehaviour
{
    private Material material;
    private float currentOpacity = 1f;
    private float targetOpacity = 1f;
    private bool isFading = false;

    // Имя property из Shader Graph
    private static readonly int OpacityID = Shader.PropertyToID("_Alpha");

    void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        // Создаём instance материала
        material = renderer.material;
        currentOpacity = material.GetFloat(OpacityID);
    }

    public void FadeOut(float opacity, float speed)
    {
        targetOpacity = opacity;
        isFading = true;
    }

    public void FadeIn(float speed)
    {
        targetOpacity = 1f;
        isFading = true;
    }

    void Update()
    {
        if (isFading)
        {
            currentOpacity = Mathf.Lerp(currentOpacity, targetOpacity, Time.deltaTime * 5f);
            material.SetFloat(OpacityID, currentOpacity);

            if (Mathf.Abs(currentOpacity - targetOpacity) < 0.01f)
            {
                currentOpacity = targetOpacity;
                material.SetFloat(OpacityID, currentOpacity);
                isFading = false;
            }
        }
    }

    void OnDestroy()
    {
        // Очистка material instance
        if (material != null)
        {
            Destroy(material);
        }
    }
}
