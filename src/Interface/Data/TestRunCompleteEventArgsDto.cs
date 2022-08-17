using System;
using System.Collections.Generic;
using Interface.Data.Logger;
using Interface.Data.Logger.Inner;

namespace Interface.Data;

// Automapper
public class TestRunCompleteEventArgsDto : EventArgsBaseDto
{
    public TestRunStatisticsDto TestRunStatistics { get; set; }

    public bool IsCanceled { get; set; }

    public bool IsAborted { get; set; }

    public Exception Error { get; set; }

    // not interested in
    // public Collection<AttachmentSet> AttachmentSets { get; set; }

    public TimeSpan ElapsedTimeInRunningTests { get; set; }

    public Dictionary<string, object> Metrics { get; set; }
}