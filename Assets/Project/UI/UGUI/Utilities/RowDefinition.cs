using System;
using UnityEngine;

[Obsolete("Класс эксперементальный и может измениться.", true)]
public class RowDefinition : MonoBehaviour
{
    [Range(0.01f, 100f)]
    public float scale = 1f;
}

