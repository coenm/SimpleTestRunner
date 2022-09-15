namespace TestRunner.Core.Tests;

using FluentAssertions;
using Xunit;

public class FreePortLocatorTests
{
    [Fact]
    public void GetAvailablePort_ShouldReturnValueWithinRange()
    {
        // arrange

        // act
        var result = FreePortLocator.GetAvailablePort();

        // assert
        result.Should().BeGreaterThanOrEqualTo(1).And.BeLessThanOrEqualTo(65536);
    }
}