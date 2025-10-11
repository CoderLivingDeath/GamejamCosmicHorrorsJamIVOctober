using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class LocalizationData
{
    [JsonProperty("keys")]
    public Dictionary<string, string> Keys { get; set; }
}


[Serializable]
public class DialogsData
{
    public Dictionary<string, DialogLog> Dialogs;
}

[Serializable]
public class DialogLog
{
    public List<DialogLine> lines;
}

[Serializable]
public class DialogLine
{
    public string titleId;
    public string textId;
}