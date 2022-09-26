namespace TestRunner.Core.Tests.Poc;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Interface.Data.Collector;
using Interface.Data.Collector.Inner;
using Interface.Data.Logger;
using Newtonsoft.Json;
using Serialization;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class DotNetTestExecutorTests
{
    private readonly StringBuilder _sb;
    private readonly EventCollection _eventCollection1;
    private readonly EventCollection _eventCollection2;
    private readonly Serialization _serialization;
    private readonly List<EventArgsBaseDto> _events = new List<EventArgsBaseDto>(1);
    private readonly DotNetTestExecutorNew _sut;
    private readonly ConsoleOutputProcessor _consoleOutputProcessor;

    public DotNetTestExecutorTests()
    {
        _sut = new DotNetTestExecutorNew();

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

        _consoleOutputProcessor = new ConsoleOutputProcessor();
        _consoleOutputProcessor.OnLine += (sender, value) =>
            {
                _sb.AppendLine("Out: " + value);
            };
        _consoleOutputProcessor.OnEvent += (sender, value) =>
            {
                _events.Add(value);
            };
    }

    [Fact]
    public async Task RunTestProject()
    {
        // arrange

        // act
        var result = await _sut.Execute(_consoleOutputProcessor.Out, _consoleOutputProcessor.Err, "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject");

        // assert
        await Verifier.Verify(new
            {
                ExitCode = result,
                Lines = _sb,
                Events = _events,
            }).AddExtraSettings(serializerSettings => serializerSettings.TypeNameHandling = TypeNameHandling.Auto);
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