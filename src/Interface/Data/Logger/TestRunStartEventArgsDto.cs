using Interface.Data.Logger.Inner;

namespace Interface.Data.Logger;

// AutoMapper
public class TestRunStartEventArgsDto : EventArgsBaseDto
{
    public TestRunCriteriaDto TestRunCriteria { get; set; }
}