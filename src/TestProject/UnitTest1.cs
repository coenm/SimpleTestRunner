using System;
using System.Threading;
using Xunit;

namespace TestProject;

public class UnitTest1
{
    // [Fact]
    // public void Test1()
    // {
    //     Thread.Sleep(TimeSpan.FromSeconds(7));
    // }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Tjepru(int i)
    {
        Thread.Sleep(TimeSpan.FromSeconds(i));
    }
}

// public class UTest2
// {
//     [Fact]
//     public void Test2()
//     {
//         Thread.Sleep(TimeSpan.FromSeconds(13));
//         throw new Exception("aap");
//     }
// }
//
// public class UnitTest123
// {
//     [Fact]
//     public void Test1()
//     {
//         Thread.Sleep(10*1000);
//     }
// }
//
// public class UTest233
// {
//     [Fact(Skip = "Jup")]
//     public void Test32()
//     {
//         Thread.Sleep(TimeSpan.FromSeconds(13));
//     }
// }

//
// public class UTest3
//     {
//         [Fact]
//     public void Test3()
//     {
//         Thread.Sleep(TimeSpan.FromSeconds(10));
//     }
// }