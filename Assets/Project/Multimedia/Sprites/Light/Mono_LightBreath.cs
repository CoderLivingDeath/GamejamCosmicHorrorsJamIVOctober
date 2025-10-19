using UnityEngine;

public class LightBreath : MonoBehaviour
{
    public Light targetLight;      // Указать Light-компонент
    public float minIntensity = 0.5f;  // Минимальная интенсивность света
    public float maxIntensity = 2.0f;  // Максимальная интенсивность света
    public float speed = 1.0f;     // Скорость пульсации

    private float timer = 0f;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime * speed;
        // Плавная пульсация интенсивности через синус
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, ((Mathf.Sin(timer) + 1f)));
        targetLight.intensity = intensity;
    }
}
