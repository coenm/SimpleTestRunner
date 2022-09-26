namespace TestRunner.Core.Tests.Poc;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Interface.Data.Collector;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;
using Serialization;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class PocTests
{
    private readonly StringBuilder _sb;
    private readonly EventCollection _eventCollection1;
    private readonly EventCollection _eventCollection2;
    private readonly Serialization _serialization;
    private readonly List<EventArgsBaseDto> _events = new List<EventArgsBaseDto>(1);

    public PocTests()
    {
        _serialization = new Serialization();
        _sb = new StringBuilder();
        _eventCollection1 = new EventCollection();
        _eventCollection1.LineAdded += (sender, value) =>
            {
                _sb.AppendLine("Out: " + value);
                ReadOnlySpan<char> line = value;

                EventArgsBaseDto? obj = _serialization.Deserialize(line);
                if (obj != null)
                {
                    _events.Add(obj);
                }
            };

        _eventCollection2 = new EventCollection();
        _eventCollection2.LineAdded += (sender, value) =>
            {
                _sb.AppendLine("Err: " + value);
            };
    }

    [Fact]
    public async Task Normal()
    {
        // arrange

        // act
        var result = await PocShellExecutor.Execute(_eventCollection1, _eventCollection2, "info");

        // assert
        await Verifier.Verify(new
            {
                Lines = _sb,
                Events = _events,
            });
        result.Should().BeFalse();
    }

    [Fact]
    public async Task Throw()
    {
        // arrange

        // act
        var result = await PocShellExecutor.Execute(_eventCollection1, _eventCollection2, "throw");

        // assert
        await Verifier.Verify(_sb);
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task Serialize_Deserialize()
    {
        // arrange
        var inputEvent = new TestCaseStartEventArgsDto
            {
                IsChildTestCase = true,
                SessionId = "ss",
                TestCaseId = Guid.Empty,
                TestCaseName = "aaa",
                TestElement = new TestCaseDto
                    {
                        CodeFilePath = "d",
                        DisplayName = "dd",
                    },
            };

        // act
        var line = _serialization.Serialize(inputEvent);
        var isTestRunnerLine = _serialization.IsTestRunnerLine(line);
        var outputEvent = _serialization.Deserialize(line) as TestCaseStartEventArgsDto;

        // assert 
        await Verifier.Verify(new
            {
                InputEvent = inputEvent,
                OutputEvent = outputEvent,
                String = line,
                IsTestRunnerLine = isTestRunnerLine,
            });
    }
}