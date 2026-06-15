using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Auth;

public sealed class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand, Result>
{
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailDispatchQueue _emailDispatchQueue;

    public ForgotPasswordCommandHandler(
        IPasswordResetService passwordResetService,
        IEmailDispatchQueue emailDispatchQueue)
    {
        _passwordResetService = passwordResetService;
        _emailDispatchQueue = emailDispatchQueue;
    }

    public async Task<Result> HandleAsync(ForgotPasswordCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.Email))
            return Result.Fail("EMAIL_REQUIRED", "Email is required.");

        var result = await _passwordResetService.GeneratePasswordResetTokenAsync(command.Email, ct);
        
        // Security rule: if user is not found, we still return success to prevent email enumeration
        if (!result.IsSuccess)
            return Result.Ok();

        var token = result.Value;
        var resetLink = BuildResetLink(command.ResetLinkBaseUrl, token, command.Email);

        var html = $@"
            <p>Olá,</p>
            <p>Você solicitou a alteração de sua senha no portal <strong>BabaPlay</strong>.</p>
            <p>Clique no link abaixo para configurar sua nova senha:</p>
            <p><a href=""{resetLink}"" style=""display: inline-block; padding: 10px 20px; color: white; background-color: #000; text-decoration: none; border-radius: 5px;"">Redefinir Senha</a></p>
            <p>Se você não fez essa solicitação, pode ignorar este e-mail.</p>
            <p>O link expirará em breve.</p>";

        await _emailDispatchQueue.EnqueueAsync(new EmailMessage(
            command.Email.Trim().ToLowerInvariant(),
            "Redefinição de Senha - BabaPlay",
            html,
            $"Acesse o link para redefinir sua senha: {resetLink}"
        ), ct);

        return Result.Ok();
    }

    private static string BuildResetLink(string baseUrl, string token, string email)
    {
        var normalizedBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? "http://localhost:5173/reset-password"
            : baseUrl.Trim();

        var separator = normalizedBaseUrl.Contains('?') ? "&" : "?";
        return $"{normalizedBaseUrl}{separator}token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";
    }
}
