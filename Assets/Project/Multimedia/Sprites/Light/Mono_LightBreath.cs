using UnityEngine;

public class LightBreath : MonoBehaviour
{
    public Light targetLight;      // ������� Light-���������
    public float minIntensity = 0.5f;  // ����������� ������������� �����
    public float maxIntensity = 2.0f;  // ������������ ������������� �����
    public float speed = 1.0f;     // �������� ���������

    private float timer = 0f;

    void Start()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();
    }

    void Update()
    {
        timer += Time.deltaTime * speed;
        // ������� ��������� ������������� ����� �����
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, ((Mathf.Sin(timer) + 1f)));
        targetLight.intensity = intensity;
    }
}
