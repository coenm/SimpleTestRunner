namespace TestRunner.Core.Tests;

using FluentAssertions;
using Xunit;

public class UnitTFreePortLocatorTests
{
    [Fact]
    public void GetAvailablePort_ShouldReturnValueWithinRange()
    {
        // arrange

        // act
        var result = FreePortLocator.GetAvailablePort();

        // assert
        result.Should().BeGreaterThan(1).And.BeLessThanOrEqualTo(65536);
    }
}