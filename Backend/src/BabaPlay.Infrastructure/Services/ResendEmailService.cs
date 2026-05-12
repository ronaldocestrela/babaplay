using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Resend;
using ApplicationEmailMessage = BabaPlay.Application.Interfaces.EmailMessage;

namespace BabaPlay.Infrastructure.Services;

public sealed class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly ResendEmailSettings _settings;

    public ResendEmailService(IResend resend, IOptions<ResendEmailSettings> settings)
    {
        _resend = resend;
        _settings = settings.Value;
    }

    public async Task<Result> SendAsync(ApplicationEmailMessage message, CancellationToken ct = default)
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            return Result.Fail("EMAIL_PROVIDER_NOT_CONFIGURED", "Resend API key is not configured.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            return Result.Fail("EMAIL_PROVIDER_NOT_CONFIGURED", "Resend sender e-mail is not configured.");

        if (string.IsNullOrWhiteSpace(message.To) || string.IsNullOrWhiteSpace(message.Subject) || string.IsNullOrWhiteSpace(message.Html))
            return Result.Fail("EMAIL_INVALID_PAYLOAD", "Email payload must include recipient, subject and html body.");

        var from = string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromEmail
            : $"{_settings.FromName} <{_settings.FromEmail}>";

        var resendMessage = new Resend.EmailMessage
        {
            From = from,
            Subject = message.Subject,
            HtmlBody = message.Html,
            TextBody = message.Text,
        };
        resendMessage.To.Add(message.To);

        ResendResponse<Guid> response;
        try
        {
            response = await _resend.EmailSendAsync(resendMessage, ct);
        }
        catch (Exception ex)
        {
            return Result.Fail("EMAIL_SEND_FAILED", $"Failed to call Resend SDK: {ex.Message}");
        }

        if (response.Success)
            return Result.Ok();

        var errorMessage = response.Exception?.Message ?? "Unknown error from Resend provider.";
        return Result.Fail(
            "EMAIL_SEND_FAILED",
            $"Resend send failed: {errorMessage}");
    }

    private string ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            return _settings.ApiKey.Trim();

        return Environment.GetEnvironmentVariable("RESEND_API_KEY")?.Trim() ?? string.Empty;
    }
}
