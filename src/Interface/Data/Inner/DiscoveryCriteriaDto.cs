using System;
using System.Collections.Generic;

namespace Interface.Data.Inner;

// automapper
public class DiscoveryCriteriaDto
{
    public TimeSpan DiscoveredTestEventTimeout { get; set; }

    public Dictionary<string, List<string>> AdapterSourceMap { get; set; }

    public string Package { get; set; }

    public long FrequencyOfDiscoveredTestsEvent { get; set; }
        
    public string RunSettings { get; set; }

    public string TestCaseFilter { get; set; }

    public TestSessionInfoDto TestSessionInfo { get; set; }
}