using UnityEngine;

[CreateAssetMenu( fileName ="LocationSettings", menuName = "Configs/LocationSettings")]
public class LocationSettings : ScriptableObject
{
    public Material WallMaterial;
    public Material DoorMaterial;

    public float MinWallAlpha = 0.3f;
    public float MaxWallAlpha = 1f;

    public float MinDoorAlpha = 0.3f;
    public float MaxDoorAlpha = 1f;
}