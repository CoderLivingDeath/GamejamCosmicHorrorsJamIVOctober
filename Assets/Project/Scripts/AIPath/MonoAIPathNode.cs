using System.Collections.Generic;
using UnityEngine;

public class MonoAIPathNode : MonoBehaviour
{
    // Теперь множество связей
    public List<MonoAIPathNode> NextNodes = new List<MonoAIPathNode>();
    public List<MonoAIPathNode> PrevNodes = new List<MonoAIPathNode>();
}
