using UnityEngine;
using System;

/// <summary>
/// Скрипт обзора монстра, использующий коллайдер-триггер.
/// </summary>
public class MonsterVisionTrigger : MonoBehaviour
{
    public event Action<Transform> TargetEntered;
    public event Action<Transform> TargetExited;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TargetEntered?.Invoke(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TargetExited?.Invoke(other.transform);
        }
    }
}
