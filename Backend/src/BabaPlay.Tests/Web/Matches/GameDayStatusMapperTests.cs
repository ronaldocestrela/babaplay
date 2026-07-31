using BabaPlay.Web.Serialization;
using FluentAssertions;
using Xunit;

namespace BabaPlay.Tests.Web.Matches;

public class GameDayStatusMapperTests
{
    [Theory]
    [InlineData("Pending", 0)]
    [InlineData("Confirmed", 1)]
    [InlineData("Cancelled", 2)]
    [InlineData("Completed", 3)]
    public void TryToNumeric_ShouldMapStatusNames(string status, int expected)
    {
        var result = GameDayStatusMapper.TryToNumeric(status, out var value);

        result.Should().BeTrue();
        value.Should().Be(expected);
    }

    [Theory]
    [InlineData("Pending", "Confirmed")]
    [InlineData("Confirmed", "Completed")]
    [InlineData("Completed", null)]
    [InlineData("Cancelled", null)]
    public void GetNextStatus_ShouldReturnExpectedTransition(string current, string? expected)
    {
        GameDayStatusMapper.GetNextStatus(current).Should().Be(expected);
    }
}
