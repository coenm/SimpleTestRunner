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
    private readonly DotNetTestExecutor _sut;
    private readonly ConsoleOutputProcessor _consoleOutputProcessor;

    public DotNetTestExecutorTests()
    {
        _sut = new DotNetTestExecutor();

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

    }

    [Fact]
    public async Task RunTestProject()
    {
        // arrange

        // act
        var result = await _sut.Execute(
            _consoleOutputProcessor.Out,
            _consoleOutputProcessor.Err,
            "C:\\Projects\\Private\\git\\SimpleTestRunner\\src\\TestProject",
            "--filter",
            "Category=single");

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
    public void Case1()
    {
        // arrange
        var line =
            "#@testrunner:22:TestResultEventArgsDto:1255:DM(v0B9hqvb075{raQb)lT+4Eb075{nN%2aa#?Oug+ZQqwI2[%v}vkIg?wP$iB(XXeNZzAhEXA$h8]h4wG&VHmS&J-C#nY)y?WG}wN[LAzdK[Lb0[W[e<%b&vrcpdz/f8%e<<L$wIct^x>qg7e(Vz6BAo6azGGDgzEEulluNu-zE))gmq=TPz!T9hoMaMTv]N2XBs{daqfuC<C#!u&x>" +
            "";//"#@testrunner:23:TestCaseEndEventArgsDto:1038:DM(B2B97i2ByPlawG<e&efX58B96=TB7E{Nb03]7gc4uLfFLsmh.!(NeJCZyhz3*yh-99}v>](Lgg+6cfKXGoefX58B96=TB7F9PzdK[Lb1V&MB.bPsr://UwjyGTluNu-zE))gpJgz&ze1rcwm5nGz/](bqE%@)A=ar<AaJK3wN[?Iy&%qqtY?.Ogbpa@wO+{/BuJxwzdm6%tY?.OgbpyRAcb/hx)Ku3diZq!nPNmjx(4rVwPI@Xvrl07i+z/pB7D)DraQb)mqMKLwO#Pli^S5xwfK5UgCQ%Jh83:ueJky}v>xY0fG7HvgDdcxvQ?eIwIN?ev>.t4a$K{cB.>(tqfuq-x>z6<wklMIwG<eVlpsQ4l$7gCA=l5rzE^rhlVm0Re<<C?wO#PxraQb)B1Q44By/GgvqEL.B-RnmwPI]YzE^Z8x(n2^z!A=]raQb)e>HaYB.bPsr://UwjyGTluNu-zE))gpJgz&ze1rcwm5nGz/](bqE%@)A=ar<AaJK3wN[?Iy&%qqtY?.Ogbpa@wO+{/BuJxwzdm6%tY?.OgbpyRAcb/hx)Ku3diZq!l${ZYy*?P:vqPN1iWddpqfuC<C#!u&x>fXKAZB[&x(mH6B5c:%x(dNgwN]dDxM53fwkV#XB-Iv*CYmk7By!#-vqH6$c%Q)GfFUyeB7Gf0v@%>eB.$P0A:i-vfFUympJgz&ze1rcwgnYNb1l-/v%8)iA-Ec]b03]VCX7xcBz(gXfe372zFshGraQb)mr&xNB-.Lxa$K{9z!9AGx(4uSvrufbi=o7Ry+c=ez/{8gwG<eVlPOESp&ZF?wN/?6tWfQaz^lKWx()P+l@DxStTS1xvrcpdz/f8%avy6/wGTuRx>qg7tWhwZB98FdtTy?Oe<%b&vrcpdz/f8%e<<L$wIct^x>qg7e(Vz6BApX(vRGZ+tTSdjB.s@:zE)]FtWfP-pCb4zByxidBz(5cwIct*A+c<<y?WD#Bs{K7B98EQwnD6aefW=4zE(N=zdm6%b04AaefX27B8#5lzB[v.i=o7R3?-";

        // act
        var result = _serialization.Deserialize(line);

        // assert
    }
}