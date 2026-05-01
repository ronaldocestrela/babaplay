using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public record LoginCommand(string Email, string Password) : ICommand<Result<AuthResponse>>;
