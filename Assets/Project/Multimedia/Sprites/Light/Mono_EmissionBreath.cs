using UnityEngine;

public class Mono_EmissionBreath : MonoBehaviour
{
    public Renderer renderer;
    public Material material; 
    private float timer = 0f;
    public float speed = 1.0f;
    public float minIntensity = 0.5f;  // ћинимальна€ интенсивность света
    public float maxIntensity = 2.0f;  // ћаксимальна€ интенсивность света

    void Start()
    {

        if (renderer == null)
         renderer = GetComponent<Renderer>();
        if (material == null)
         material = renderer.material;


    }

    void Update()
    {
        timer += Time.deltaTime * speed;
        // ѕлавна€ пульсаци€ интенсивности через синус
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, ((Mathf.Sin(timer) + 1f) / 2f) + Random.Range(-0.1f, 0.1f));
        material.SetFloat("_GlowIntensity", intensity);

    }
}