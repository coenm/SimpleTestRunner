namespace Interface.Data.Collector;

using Interface.Data.Collector.Base;

public class TestCaseEndEventArgsDto: TestCaseEventArgsBaseDto
{
    public TestOutcome TestOutcome { get; set; }
}