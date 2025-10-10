using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class LocalizationData
{
    [JsonProperty("keys")]
    public KeysContainer Keys { get; set; }
}

public class KeysContainer
{
    [JsonProperty("Titles")]
    public Dictionary<string, string> Titles { get; set; }

    [JsonProperty("Messages")]
    public Dictionary<string, string> Messages { get; set; }
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
    public string title;
    public string textId;
}