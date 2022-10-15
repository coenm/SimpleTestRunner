using Interface.Data.Logger.Inner;

namespace Interface.Data.Logger;

public class TestRunStartEventArgsDto : EventArgsBaseDto
{
    public TestRunCriteriaDto TestRunCriteria { get; set; }
}