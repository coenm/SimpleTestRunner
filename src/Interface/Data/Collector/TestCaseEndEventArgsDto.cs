using Interface.Data.Collector.Base;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;

namespace Interface.Data.Collector;

// AutoMapper
public class TestCaseEndEventArgsDto: TestCaseEventArgsBaseDto
{
    public TestOutcome TestOutcome { get; set; }
}