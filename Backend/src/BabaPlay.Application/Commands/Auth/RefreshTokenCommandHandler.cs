using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public sealed class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<Result<AuthResponse>> HandleAsync(RefreshTokenCommand command, CancellationToken cancellationToken = default)
    {
        var stored = await _refreshTokenRepository.FindAsync(command.RefreshToken, cancellationToken);
        if (stored is null)
            return Result.Fail<AuthResponse>("INVALID_TOKEN", "Refresh token is invalid.");

        if (stored.IsRevoked)
            return Result.Fail<AuthResponse>("INVALID_TOKEN", "Refresh token is invalid.");

        if (stored.ExpiresAt < DateTime.UtcNow)
            return Result.Fail<AuthResponse>("TOKEN_EXPIRED", "Refresh token has expired.");

        var user = await _userRepository.FindByIdAsync(stored.UserId, cancellationToken);
        if (user is null || !user.IsActive)
            return Result.Fail<AuthResponse>("INVALID_TOKEN", "Refresh token is invalid.");

        // Rotate: revoke old token, issue new pair
        await _refreshTokenRepository.RevokeAsync(command.RefreshToken, cancellationToken);

        var roles = await _userRepository.GetRolesAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpiresInDays);

        await _refreshTokenRepository.AddAsync(newRefreshToken, user.Id, expiresAt, cancellationToken);

        return Result.Ok(new AuthResponse(accessToken, newRefreshToken, _tokenService.AccessTokenExpiresInSeconds));
    }
}
