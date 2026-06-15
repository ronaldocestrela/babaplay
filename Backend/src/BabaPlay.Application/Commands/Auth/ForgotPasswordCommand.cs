using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public record ForgotPasswordCommand(string Email, string ResetLinkBaseUrl) : ICommand<Result>;
