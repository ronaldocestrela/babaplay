using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public sealed class LoginCommandHandler : ICommandHandler<LoginCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public LoginCommandHandler(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<Result<AuthResponse>> HandleAsync(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.FindByEmailAsync(command.Email, cancellationToken);
        if (user is null)
            return Result.Fail<AuthResponse>("INVALID_CREDENTIALS", "Invalid email or password.");

        if (!user.IsActive)
            return Result.Fail<AuthResponse>("USER_INACTIVE", "User account is inactive.");

        var isValidPassword = await _userRepository.CheckPasswordAsync(user.Id, command.Password, cancellationToken);
        if (!isValidPassword)
            return Result.Fail<AuthResponse>("INVALID_CREDENTIALS", "Invalid email or password.");

        var roles = await _userRepository.GetRolesAsync(user.Id, cancellationToken);
        var accessToken = _tokenService.GenerateAccessToken(user.Id, user.Email, roles);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddDays(_tokenService.RefreshTokenExpiresInDays);

        await _refreshTokenRepository.AddAsync(refreshToken, user.Id, expiresAt, cancellationToken);

        return Result.Ok(new AuthResponse(accessToken, refreshToken, _tokenService.AccessTokenExpiresInSeconds));
    }
}
