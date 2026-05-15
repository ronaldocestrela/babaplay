using BabaPlay.Application.Interfaces;
using BabaPlay.Application.Queries.Ping;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application;

public class PingQueryHandlerTests
{
    private readonly Mock<IApiReadinessProbe> _readinessProbe = new();
    private readonly PingQueryHandler _handler;

    public PingQueryHandlerTests()
    {
        _handler = new PingQueryHandler(_readinessProbe.Object);
    }

    [Fact]
    public async Task Handle_MasterDatabaseReady_ShouldReturnHealthyStatus()
    {
        // Arrange
        _readinessProbe
            .Setup(x => x.IsMasterDatabaseReadyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(new PingQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be("healthy");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task Handle_MasterDatabaseUnavailable_ShouldReturnUnhealthyStatus()
    {
        // Arrange
        _readinessProbe
            .Setup(x => x.IsMasterDatabaseReadyAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(new PingQuery());

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be("unhealthy");
        result.Value.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
