using UnityEngine;

public class Mono_EmissionFlickering : MonoBehaviour
{
    public Renderer renderer;
    public Material material; 
    public Light light; 
    public float maxWait = 1f;     // Максимальная пауза между вспышками
    public float maxFlicker = 0.2f; // Продолжительность вспышки
    private float timer = 0f;
    private float interval = 0f;
    private float defaultEmission = 1.11f;
    private float defaultIntensity = 0.1f;

    void Start()
    {
        if (light == null)
            light = GetComponentInChildren<Light>();
        if (renderer == null)
         renderer = GetComponent<Renderer>();
        if (material == null)
         material = renderer.material;

        interval = Random.Range(0f, maxWait);

    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > interval)
        {
            float delta = Random.Range(-0.1f, 0.1f);
            material.SetFloat("_GlowIntensity", defaultEmission + delta);
            float intensity = defaultIntensity + delta/10;
            light.intensity = intensity;
            interval = Random.Range(0f, maxFlicker);
            timer = 0f;
        }
    }
}