using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepo.Object, _tokenService.Object, _refreshTokenRepo.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnAccessAndRefreshTokens()
    {
        // Arrange
        var user = new UserAuthDto("user-id", "test@email.com", IsActive: true);
        _userRepo.Setup(r => r.FindByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _userRepo.Setup(r => r.CheckPasswordAsync("user-id", "ValidPass123!", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(true);
        _userRepo.Setup(r => r.GetRolesAsync("user-id", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(Array.Empty<string>());
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>()))
                     .Returns("access-token-value");
        _tokenService.Setup(t => t.GenerateRefreshToken())
                     .Returns("refresh-token-value");
        _tokenService.Setup(t => t.AccessTokenExpiresInSeconds).Returns(3600);
        _tokenService.Setup(t => t.RefreshTokenExpiresInDays).Returns(30);

        // Act
        var result = await _handler.HandleAsync(new LoginCommand("test@email.com", "ValidPass123!"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("access-token-value");
        result.Value.RefreshToken.Should().Be("refresh-token-value");
        result.Value.ExpiresIn.Should().Be(3600);
        result.Value.TokenType.Should().Be("Bearer");
        _refreshTokenRepo.Verify(r => r.AddAsync(
            "refresh-token-value", "user-id", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ShouldReturnInvalidCredentials()
    {
        // Arrange — e-mail desconhecido, repositório retorna null
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync((UserAuthDto?)null);

        // Act
        var result = await _handler.HandleAsync(new LoginCommand("nobody@email.com", "Pass123!"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CREDENTIALS");
        // Não deve revelar que o usuário não existe (user enumeration)
        result.ErrorMessage.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_WrongPassword_ShouldReturnInvalidCredentials()
    {
        // Arrange
        var user = new UserAuthDto("user-id", "test@email.com", IsActive: true);
        _userRepo.Setup(r => r.FindByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);
        _userRepo.Setup(r => r.CheckPasswordAsync("user-id", "WrongPass", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(new LoginCommand("test@email.com", "WrongPass"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnUserInactiveError()
    {
        // Arrange
        var user = new UserAuthDto("user-id", "test@email.com", IsActive: false);
        _userRepo.Setup(r => r.FindByEmailAsync("test@email.com", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

        // Act
        var result = await _handler.HandleAsync(new LoginCommand("test@email.com", "AnyPass"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("USER_INACTIVE");
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldStoreRefreshTokenWithFutureExpiry()
    {
        // Arrange
        var user = new UserAuthDto("user-id", "test@email.com", IsActive: true);
        _userRepo.Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.CheckPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _userRepo.Setup(r => r.GetRolesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<string>());
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns("tok");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("ref");
        _tokenService.Setup(t => t.AccessTokenExpiresInSeconds).Returns(3600);
        _tokenService.Setup(t => t.RefreshTokenExpiresInDays).Returns(30);

        // Act
        await _handler.HandleAsync(new LoginCommand("test@email.com", "Pass123!"));

        // Assert — expiry deve ser no futuro
        _refreshTokenRepo.Verify(r => r.AddAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.Is<DateTime>(d => d > DateTime.UtcNow),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
