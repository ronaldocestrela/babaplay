using BabaPlay.Application.Commands.Auth;
using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using FluentAssertions;
using Moq;

namespace BabaPlay.Tests.Unit.Application.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _handler = new RefreshTokenCommandHandler(_refreshTokenRepo.Object, _userRepo.Object, _tokenService.Object);
    }

    [Fact]
    public async Task Handle_ValidToken_ShouldRotateAndReturnNewTokenPair()
    {
        // Arrange
        var stored = new StoredRefreshTokenDto("old-refresh", "user-id", DateTime.UtcNow.AddDays(10), IsRevoked: false);
        var user = new UserAuthDto("user-id", "user@email.com", IsActive: true);
        _refreshTokenRepo.Setup(r => r.FindAsync("old-refresh", It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        _userRepo.Setup(r => r.FindByIdAsync("user-id", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _userRepo.Setup(r => r.GetRolesAsync("user-id", It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<string>());
        _tokenService.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>())).Returns("new-access");
        _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("new-refresh");
        _tokenService.Setup(t => t.AccessTokenExpiresInSeconds).Returns(3600);
        _tokenService.Setup(t => t.RefreshTokenExpiresInDays).Returns(30);

        // Act
        var result = await _handler.HandleAsync(new RefreshTokenCommand("old-refresh"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.AccessToken.Should().Be("new-access");
        result.Value.RefreshToken.Should().Be("new-refresh");
        // Token antigo revogado (rotação)
        _refreshTokenRepo.Verify(r => r.RevokeAsync("old-refresh", It.IsAny<CancellationToken>()), Times.Once);
        // Novo token armazenado
        _refreshTokenRepo.Verify(r => r.AddAsync("new-refresh", "user-id", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_TokenNotFound_ShouldReturnInvalidToken()
    {
        // Arrange
        _refreshTokenRepo.Setup(r => r.FindAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync((StoredRefreshTokenDto?)null);

        // Act
        var result = await _handler.HandleAsync(new RefreshTokenCommand("unknown-token"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task Handle_RevokedToken_ShouldReturnInvalidToken()
    {
        // Arrange
        var stored = new StoredRefreshTokenDto("revoked", "user-id", DateTime.UtcNow.AddDays(10), IsRevoked: true);
        _refreshTokenRepo.Setup(r => r.FindAsync("revoked", It.IsAny<CancellationToken>())).ReturnsAsync(stored);

        // Act
        var result = await _handler.HandleAsync(new RefreshTokenCommand("revoked"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TOKEN");
    }

    [Fact]
    public async Task Handle_ExpiredToken_ShouldReturnTokenExpired()
    {
        // Arrange
        var stored = new StoredRefreshTokenDto("expired", "user-id", DateTime.UtcNow.AddSeconds(-1), IsRevoked: false);
        _refreshTokenRepo.Setup(r => r.FindAsync("expired", It.IsAny<CancellationToken>())).ReturnsAsync(stored);

        // Act
        var result = await _handler.HandleAsync(new RefreshTokenCommand("expired"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("TOKEN_EXPIRED");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnInvalidToken()
    {
        // Arrange — token válido mas usuário foi desativado após emissão
        var stored = new StoredRefreshTokenDto("tok", "user-id", DateTime.UtcNow.AddDays(5), IsRevoked: false);
        var inactiveUser = new UserAuthDto("user-id", "user@email.com", IsActive: false);
        _refreshTokenRepo.Setup(r => r.FindAsync("tok", It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        _userRepo.Setup(r => r.FindByIdAsync("user-id", It.IsAny<CancellationToken>())).ReturnsAsync(inactiveUser);

        // Act
        var result = await _handler.HandleAsync(new RefreshTokenCommand("tok"));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_TOKEN");
    }
}
