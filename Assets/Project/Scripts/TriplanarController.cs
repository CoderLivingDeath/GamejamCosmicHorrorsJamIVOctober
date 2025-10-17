using UnityEngine;

[ExecuteAlways]
public class TriplanarController : MonoBehaviour
{
    [SerializeField] private string shaderParameterName = "_MyVector";
    [SerializeField] private Vector3 parameterValue = Vector3.one;

    private Material material;

    private void Awake()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            material = renderer.material;
        }
    }

    private void OnValidate()
    {
        if (material != null && !string.IsNullOrEmpty(shaderParameterName))
        {
            // SetVector принимает Vector4, Vector3 конвертируется автоматически
            material.SetVector(shaderParameterName, parameterValue);
        }
    }
}
