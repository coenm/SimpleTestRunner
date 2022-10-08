namespace TestRunner.Core.Tests.Poc;

using System;
using System.Collections.Generic;
using System.IO;
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
    public async Task Color()
    {
        // arrange
        using var stream1 = new MemoryStream();
        using var stream2 = new MemoryStream();

        // act
        var colorResult = await PocShellExecutor.Execute(stream1, "color");
        var blackResult = await PocShellExecutor.Execute(stream2, "black");

        // assert
        var bytes1 = stream1.ToArray();
        var bytes2 = stream2.ToArray();
        bytes1.Should().BeEquivalentTo(bytes2);

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

    [Fact]
    public async Task FromFile1()
    {
        // arrange
        int count = 0;
        var lines = await File.ReadAllLinesAsync("Poc\\output4.txt");

        string line = string.Empty;

        // act
        try
        {
            foreach (var line1 in lines)
            {
                line = line1;
                if (_serialization.IsTestRunnerLine(line))
                {
                    // if (line[1..].Contains("#@testrunner"))
                    if (line.Contains("Microsoft (R) Test Execution Command Line Tool Version 17.3.0 (x64)"))
                    {
                        continue;
                    }

                    count++;
                    EventArgsBaseDto? output = _serialization.Deserialize(line);

                    // assert
                    output.Should().NotBeNull();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       

        count.Should().BeGreaterThan(20);
    }
}