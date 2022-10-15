using Interface.Data.Collector.Base;

namespace Interface.Data.Collector;

public class TestHostLaunchedEventArgsDto : DataCollectionEventArgsBaseDto
{
    public int TestHostProcessId { get; set; }
}