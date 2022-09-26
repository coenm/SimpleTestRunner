using System;
using System.Threading;
using Xunit;

namespace TestProject;

using Xunit.Abstractions;

public class UnitTest1
{
    private readonly ITestOutputHelper _output;

    public UnitTest1(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
        _output.WriteLine("ctor");
    }

    [Fact]
    public void TestA()
    {
        _output.WriteLine("test a message before");
        Thread.Sleep(TimeSpan.FromMilliseconds(700));
        _output.WriteLine("test a message after");
    }

    [Fact]
    [Trait("Category", "single")]
    public void TestB()
    {
        Assert.Equal(true, true);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void TestC(int i)
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(i * 100));

        Assert.NotEqual(6, i);
    }
}