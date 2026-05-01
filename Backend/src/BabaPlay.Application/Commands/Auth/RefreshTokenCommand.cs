using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken) : ICommand<Result<AuthResponse>>;
