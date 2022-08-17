using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Interface.Data.Collector.Inner;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

namespace Interface.Data.Logger.Inner;

// AutoMapper
public class TestResultDto : TestObjectDto
{
    public TestCaseDto TestCase { get; set; }

    // skip
    //public List<AttachmentSet> Attachments { get; set; }

    public TestOutcome Outcome { get; set; }

    public string ErrorMessage { get; set; }

    public string ErrorStackTrace { get; set; }

    public string DisplayName { get; set; }

    public List<TestResultMessageDto> Messages { get; set; }

    public string ComputerName { get; set; }

    public TimeSpan Duration { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public DateTimeOffset EndTime { get; set; }
}

public abstract class TestObjectDto
{
    // todo
    // properties list
}