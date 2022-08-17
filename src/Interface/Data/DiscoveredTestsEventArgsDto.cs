using System.Collections.Generic;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;

namespace Interface.Data;

// automapper
public class DiscoveredTestsEventArgsDto : EventArgsBaseDto
{
    public List<TestCaseDto> DiscoveredTestCases { get; set; }
}