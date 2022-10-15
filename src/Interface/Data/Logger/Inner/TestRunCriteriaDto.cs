using System.Collections.Generic;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger.Base;

namespace Interface.Data.Logger.Inner;

public class TestRunCriteriaDto : BaseTestRunCriteriaBaseDto
{
    //public Dictionary<string, IEnumerable<string>> AdapterSourceMap { get; set; }

    // check ienumerable
    public IEnumerable<TestCaseDto> Tests { get; set; }

    public string TestCaseFilter { get; set; }

    // public FilterOptions FilterOptions { get; set; }

    public bool HasSpecificSources { get; set; }
}