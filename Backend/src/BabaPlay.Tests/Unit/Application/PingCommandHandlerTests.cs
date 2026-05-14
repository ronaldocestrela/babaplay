using BabaPlay.Application.Common;
using BabaPlay.Application.Commands.Ping;
using FluentAssertions;

namespace BabaPlay.Tests.Unit.Application;

public class PingCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnSuccessWithPongMessage()
    {
        // Arrange — Red: este teste vai falhar até o handler existir
        var handler = new PingCommandHandler();
        var command = new PingCommand("test-sender");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrWhiteSpace();
        result.Value.Should().Contain("pong");
    }

    [Fact]
    public async Task Handle_EmptySender_ShouldReturnFailure()
    {
        // Arrange
        var handler = new PingCommandHandler();
        var command = new PingCommand(string.Empty);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }
}
