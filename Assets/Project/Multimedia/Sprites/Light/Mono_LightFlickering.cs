using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light targetLight; // Ссылка на Light-компонент
    public float maxWait = 1f;     // Максимальная пауза между вспышками
    public float maxFlicker = 0.2f; // Продолжительность вспышки
    private float timer = 0f;
    private float interval = 0f;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();
        interval = Random.Range(0f, maxWait);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > interval)
        {
            targetLight.enabled = !targetLight.enabled;
            if (targetLight.enabled)
                interval = Random.Range(0f, maxWait);
            else
                interval = Random.Range(0f, maxFlicker);
            timer = 0f;
        }
    }
}