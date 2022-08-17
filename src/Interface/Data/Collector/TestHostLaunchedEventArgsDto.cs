using Interface.Data.Collector.Base;

namespace Interface.Data.Collector;

// AutoMapper
public class TestHostLaunchedEventArgsDto : DataCollectionEventArgsBaseDto
{
    public int TestHostProcessId { get; set; }
}