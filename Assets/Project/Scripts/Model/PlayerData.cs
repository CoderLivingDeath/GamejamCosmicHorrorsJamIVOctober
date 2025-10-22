using System.Collections.Generic;

public partial class PlayerData
{
    public Dictionary<string, bool> KeyCard { get; set; } = new Dictionary<string, bool>();

    public PlayerData()
    {
        if (KeyCard == null)
            KeyCard = new Dictionary<string, bool>();
    }
}
