using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Auth;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IPasswordResetService> _passwordResetServiceMock = new();
    private readonly Mock<IEmailDispatchQueue> _emailDispatchQueueMock = new();
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _handler = new ForgotPasswordCommandHandler(
            _passwordResetServiceMock.Object,
            _emailDispatchQueueMock.Object);
    }

    [Fact]
    public async Task Handle_EmptyEmail_ShouldReturnFailure()
    {
        // Act
        var result = await _handler.HandleAsync(new ForgotPasswordCommand("", "http://localhost"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("EMAIL_REQUIRED");
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnSuccessWithoutSendingEmail()
    {
        // Arrange
        _passwordResetServiceMock
            .Setup(s => s.GeneratePasswordResetTokenAsync("unknown@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Fail("USER_NOT_FOUND", "User not found"));

        // Act
        var result = await _handler.HandleAsync(new ForgotPasswordCommand("unknown@email.com", "http://localhost"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        _emailDispatchQueueMock.Verify(
            q => q.EnqueueAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_UserFound_ShouldGenerateTokenAndEnqueueEmail()
    {
        // Arrange
        const string email = "user@email.com";
        const string token = "reset-token-xyz";
        const string baseUrl = "http://localhost:5173/reset-password";

        _passwordResetServiceMock
            .Setup(s => s.GeneratePasswordResetTokenAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Ok(token));

        // Act
        var result = await _handler.HandleAsync(new ForgotPasswordCommand(email, baseUrl));

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        _emailDispatchQueueMock.Verify(
            q => q.EnqueueAsync(
                It.Is<EmailMessage>(m => 
                    m.To == email && 
                    m.Subject.Contains("Redefinição de Senha") &&
                    m.Html.Contains("token=reset-token-xyz") &&
                    m.Html.Contains("email=user%40email.com")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
