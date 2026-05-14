using BabaPlay.Application.Queries.Ping;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Application;

public class PingQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnServerStatus()
    {
        // Arrange — Red: este teste vai falhar até o handler existir
        var handler = new PingQueryHandler();
        var query = new PingQuery();

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be("healthy");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
