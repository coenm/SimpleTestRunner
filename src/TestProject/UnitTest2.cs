namespace TestProject;

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class UnitTest2
{
    [Fact]
    public void TestD()
    {
        Thread.Sleep(TimeSpan.FromMilliseconds(700));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public async Task TestE(int methodArgument1)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(methodArgument1*1000));

        Assert.NotEqual(2, methodArgument1);
    }
}