using System;
using Interface.Data.Logger.Inner;

namespace Interface.Data.Collector.Inner;

public class TestCaseDto : TestObjectDto
{
    public Guid Id { get; set; }

    public string FullyQualifiedName { get; set; }

    public string DisplayName { get; set; }

    public Uri ExecutorUri { get; set; }

    public string CodeFilePath { get; set; }

    public string Source { get; set; }

    public int LineNumber { get; set; }
}