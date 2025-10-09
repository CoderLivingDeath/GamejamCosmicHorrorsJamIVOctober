using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnxietyDistortionEffect : MonoBehaviour
{
    [SerializeField]
    private Volume postProcessVolume;

    private Material postProcessMaterial;
    private float elapsedTime;

    // ��������� ��� �������� � ������
    private static readonly int _DistortionAmount = Shader.PropertyToID("_DistortionAmount");
    private static readonly int _ChromaticOffset = Shader.PropertyToID("_ChromaticOffset");
    private static readonly int _NoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int _IsActive = Shader.PropertyToID("_IsActive");

    [SerializeField]
    private UniversalRendererData rendererData; // ���� ������� ����� PC_Renderer

    public bool isActive = false;

    void Start()
    {
        if (postProcessVolume == null)
        {
            Debug.LogError("PostProcess Volume is not assigned.");
            enabled = false;
            return;
        }

        foreach (var feature in rendererData.rendererFeatures)
        {
            // ��� ��� � ���������� Render Feature
            if (feature.name == "FullScreenPassRendererFeature")
            {
                var fsFeature = feature as FullScreenPassRendererFeature;
                if (fsFeature != null)
                {
                    // ������ ��������
                    postProcessMaterial = fsFeature.passMaterial;
                    // ������ ����� ������������ passMat.SetFloat, SetInt, SetColor � �.�.
                }
            }
        }

        // ��������� �������� �� Full Screen Pass Renderer Feature (��������� �������� ����� URP Render Features)
        // ����� �����������, ��� � Volume ���������� �������� � ������ �������� (����������� � Full Screen Pass)
        //var rendererFeature = GetComponent<UniversalAdditionalCameraData>().scriptableRenderer
        //    .rendererFeatures.Find(r => r.name == "AnxietyDistortionFeature") as ScriptableRendererFeature;

        // ��� ������������� ����� ��������� ������ �� �������� ����� ����� ����������

        // ��� ������� ���������� ������ ��������� ����� volume (�� �������� ����� ����������� ������ � ���������)
    }

    void Update()
    {
        
        elapsedTime += Time.deltaTime;

        // ������������ ��������� ��� ���������� �������
        float distortion = Mathf.Sin(elapsedTime * 3f) * 0.15f + 0.15f;
        float chromaticShift = Mathf.Cos(elapsedTime * 2f) * 0.02f;
        float noise = Mathf.PerlinNoise(elapsedTime * 1.5f, 0f) * 0.1f;

        if (postProcessMaterial != null)
        {
            if (isActive)
            {
                postProcessMaterial.SetInt(_IsActive, 1);
                //postProcessMaterial.SetFloat(_DistortionAmount, distortion);
                //postProcessMaterial.SetFloat(_ChromaticOffset, chromaticShift);
                //postProcessMaterial.SetFloat(_NoiseIntensity, noise);

            } else
            {
                postProcessMaterial.SetInt(_IsActive, 0);
            }
        }
    }

    private void OnDestroy()
    {
        postProcessMaterial.SetInt(_IsActive, 0);
    }
}
