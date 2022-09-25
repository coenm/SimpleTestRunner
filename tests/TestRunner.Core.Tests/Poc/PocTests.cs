namespace TestRunner.Core.Tests.Poc;

using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using VerifyXunit;
using Xunit;

[UsesVerify]
public class PocTests
{
    private readonly StringBuilder _sb;
    private readonly EventCollection _eventCollection1;
    private readonly EventCollection _eventCollection2;

    public PocTests()
    {
        _sb = new StringBuilder();
        _eventCollection1 = new EventCollection();
        _eventCollection1.LineAdded += (sender, value) =>
            {
                _sb.AppendLine("Out: " + value);
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
        await Verifier.Verify(_sb);
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
}