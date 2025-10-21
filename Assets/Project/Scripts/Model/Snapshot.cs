using System;
using System.Collections.Generic;

[Serializable]
public class Snapshot
{
    public Dictionary<Guid, IList<ComponentParameterSnapshot>> componentParameterSnapshots;
    public Snapshot(Guid guid, IList<ComponentParameterSnapshot> parametres)
    {
        componentParameterSnapshots = new()
        {
            { guid, parametres }
        };
    }
}
