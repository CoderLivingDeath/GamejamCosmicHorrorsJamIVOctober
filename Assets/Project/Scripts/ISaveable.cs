using System;
using System.Collections.Generic;

public interface ISaveable
{
    IList<ComponentParameterSnapshot> Capture();
    Guid Guid { get; }
    void Restore(IList<ComponentParameterSnapshot> snapshots);
}
