using System;
using System.Threading;
using Xunit;

namespace TestProject;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        Thread.Sleep(TimeSpan.FromSeconds(7));
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Test2(int i)
    {
        Thread.Sleep(TimeSpan.FromSeconds(i));

        Assert.NotEqual(6, i);
    }
}