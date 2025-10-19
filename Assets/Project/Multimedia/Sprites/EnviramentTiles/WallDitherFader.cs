using System.Collections.Generic;
using UnityEngine;

public class WallDitherFader : MonoBehaviour
{
    public Transform player;
    public float fadeSpeed = 5f;
    public float targetOpacity = 0.3f; // 0 = полностью прозрачно, 1 = непрозрачно

    private List<DitherFadableWall> currentlyFadedWalls = new List<DitherFadableWall>();

    void Update()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        RaycastHit[] hits = Physics.RaycastAll(transform.position, directionToPlayer, distanceToPlayer);

        List<DitherFadableWall> newFadedWalls = new List<DitherFadableWall>();

        foreach (var hit in hits)
        {
            if (hit.transform != player && hit.transform.TryGetComponent(out DitherFadableWall wall))
            {
                wall.FadeOut(targetOpacity, fadeSpeed);
                newFadedWalls.Add(wall);
            }
        }

        // Восстанавливаем непрозрачность
        foreach (var wall in currentlyFadedWalls)
        {
            if (!newFadedWalls.Contains(wall))
            {
                wall.FadeIn(fadeSpeed);
            }
        }

        currentlyFadedWalls = newFadedWalls;
    }
}
