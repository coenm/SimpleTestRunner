using System.Collections.Generic;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;

namespace Interface.Data;

public class DiscoveredTestsEventArgsDto : EventArgsBaseDto
{
    public List<TestCaseDto> DiscoveredTestCases { get; set; }
}