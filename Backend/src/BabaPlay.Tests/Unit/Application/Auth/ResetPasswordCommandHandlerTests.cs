using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Auth;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IPasswordResetService> _passwordResetServiceMock = new();
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _handler = new ResetPasswordCommandHandler(_passwordResetServiceMock.Object);
    }

    [Fact]
    public async Task Handle_EmptyEmail_ShouldReturnFailure()
    {
        var result = await _handler.HandleAsync(new ResetPasswordCommand("", "token", "password"));
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_REQUIRED");
    }

    [Fact]
    public async Task Handle_EmptyToken_ShouldReturnFailure()
    {
        var result = await _handler.HandleAsync(new ResetPasswordCommand("email@test.com", "", "password"));
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TOKEN_REQUIRED");
    }

    [Fact]
    public async Task Handle_EmptyPassword_ShouldReturnFailure()
    {
        var result = await _handler.HandleAsync(new ResetPasswordCommand("email@test.com", "token", ""));
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("PASSWORD_REQUIRED");
    }

    [Fact]
    public async Task Handle_ResetPasswordServiceFails_ShouldReturnFailure()
    {
        // Arrange
        _passwordResetServiceMock
            .Setup(s => s.ResetPasswordAsync("email@test.com", "token", "new-password", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("RESET_PASSWORD_FAILED", "Invalid token"));

        // Act
        var result = await _handler.HandleAsync(new ResetPasswordCommand("email@test.com", "token", "new-password"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("RESET_PASSWORD_FAILED");
    }

    [Fact]
    public async Task Handle_ResetPasswordServiceSucceeds_ShouldReturnSuccess()
    {
        // Arrange
        _passwordResetServiceMock
            .Setup(s => s.ResetPasswordAsync("email@test.com", "token", "new-password", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        var result = await _handler.HandleAsync(new ResetPasswordCommand("email@test.com", "token", "new-password"));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
