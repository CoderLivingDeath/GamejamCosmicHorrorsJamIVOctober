using UnityEngine;

[CreateAssetMenu(fileName = "MonoPathAnimationEditorSettings", menuName = "Configs/MonoPathAnimationEditorSettings")]
public class MonoPathAnimationEditorSettings : ScriptableObject
{
    [Header("Colors")]
    public Color boundsColorActive = new Color(0, 1, 0, 0.25f);
    public Color boundsColorInactive = new Color(1, 0, 0, 0.5f);
    public Color boundsColorFirstPointActive = new Color(0, 0.5f, 1f, 0.4f);
    public Color boundsColorLastPointActive = new Color(1f, 0.5f, 0, 0.4f);
    public Color boundsColorBad = new Color(1f, 0.3f, 0.3f, 0.6f);
    public Color labelColor = Color.white;
    public Color edgeColor = Color.green;

    [Header("Label and button")]
    public float lineThickness = 2f;
    public float labelWidth = 150f;
    public Vector3 labelPositionOffset = new Vector3(1.1f, 0f, 0f);
    public Vector3 buttonPositionOffset = new Vector3(2.1f, 0f, 0f);
}
