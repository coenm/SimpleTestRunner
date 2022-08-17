using System;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;

namespace Interface.Data.Collector.Base;

public abstract class TestCaseEventArgsBaseDto : EventArgsBaseDto
{
    public Guid TestCaseId { get; set; }

    public string TestCaseName { get; set; }

    public bool IsChildTestCase { get; set; }

    public TestCaseDto TestElement { get; set; }
}