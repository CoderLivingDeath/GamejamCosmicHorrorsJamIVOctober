using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AnxietyDistortionEffect : MonoBehaviour
{
    [SerializeField]
    private Volume postProcessVolume;

    private Material postProcessMaterial;
    private float elapsedTime;

    // Параметры для передачи в шейдер
    private static readonly int _DistortionAmount = Shader.PropertyToID("_DistortionAmount");
    private static readonly int _ChromaticOffset = Shader.PropertyToID("_ChromaticOffset");
    private static readonly int _NoiseIntensity = Shader.PropertyToID("_NoiseIntensity");
    private static readonly int _IsActive = Shader.PropertyToID("_IsActive");

    [SerializeField]
    private UniversalRendererData rendererData; // сюда присвой ассет PC_Renderer

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
            // Имя как в настройках Render Feature
            if (feature.name == "FullScreenPassRendererFeature")
            {
                var fsFeature = feature as FullScreenPassRendererFeature;
                if (fsFeature != null)
                {
                    // Получи материал
                    postProcessMaterial = fsFeature.passMaterial;
                    // Теперь можно использовать passMat.SetFloat, SetInt, SetColor и т.д.
                }
            }
        }

        // Извлекаем материал из Full Screen Pass Renderer Feature (создается отдельно через URP Render Features)
        // Здесь предположим, что в Volume содержится материал с нужным шейдером (назначенным в Full Screen Pass)
        //var rendererFeature = GetComponent<UniversalAdditionalCameraData>().scriptableRenderer
        //    .rendererFeatures.Find(r => r.name == "AnxietyDistortionFeature") as ScriptableRendererFeature;

        // При необходимости можно сохранять ссылку на материал через среду реализации

        // Для примера используем прямое обращение через volume (на практике нужно реализовать доступ к материалу)
    }

    void Update()
    {
        
        elapsedTime += Time.deltaTime;

        // Пульсирующее искажение для тревожного эффекта
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
