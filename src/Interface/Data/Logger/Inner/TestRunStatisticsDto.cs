using System.Collections.Generic;

namespace Interface.Data.Logger.Inner;

public class TestRunStatisticsDto
{
    // private long this[TestOutcome testOutcome] { get; set; }

    public Dictionary<TestOutcome, long> Stats { get; set; }

    public long ExecutedTests { get; set; }
}