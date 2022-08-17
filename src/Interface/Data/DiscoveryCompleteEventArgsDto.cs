using Interface.Data.Logger;
using System.Collections.Generic;

namespace Interface.Data;

// automapper
public class DiscoveryCompleteEventArgsDto : EventArgsBaseDto
{
    public bool IsAborted { get; set; }

    public long TotalCount { get; set; }

    public Dictionary<string, object> Metrics { get; set; }
}