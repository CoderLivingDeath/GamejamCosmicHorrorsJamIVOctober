using System;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class ComponentParameter
{
    [JsonIgnore]
    public Component TargetComponent;
    public string ParameterName;
}
