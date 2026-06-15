using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public record ResetPasswordCommand(string Email, string Token, string Password) : ICommand<Result>;
