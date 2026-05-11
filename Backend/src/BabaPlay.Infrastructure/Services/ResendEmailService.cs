using System.Net.Http.Json;
using BabaPlay.Application.Common;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace BabaPlay.Infrastructure.Services;

public sealed class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly ResendEmailSettings _settings;

    public ResendEmailService(HttpClient httpClient, IOptions<ResendEmailSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var apiKey = ResolveApiKey();
        if (string.IsNullOrWhiteSpace(apiKey))
            return Result.Fail("EMAIL_PROVIDER_NOT_CONFIGURED", "Resend API key is not configured.");

        if (string.IsNullOrWhiteSpace(_settings.FromEmail))
            return Result.Fail("EMAIL_PROVIDER_NOT_CONFIGURED", "Resend sender e-mail is not configured.");

        if (string.IsNullOrWhiteSpace(message.To) || string.IsNullOrWhiteSpace(message.Subject) || string.IsNullOrWhiteSpace(message.Html))
            return Result.Fail("EMAIL_INVALID_PAYLOAD", "Email payload must include recipient, subject and html body.");

        var baseUrl = string.IsNullOrWhiteSpace(_settings.BaseUrl)
            ? "https://api.resend.com"
            : _settings.BaseUrl;
        var endpoint = new Uri(new Uri(baseUrl, UriKind.Absolute), "emails");

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        var from = string.IsNullOrWhiteSpace(_settings.FromName)
            ? _settings.FromEmail
            : $"{_settings.FromName} <{_settings.FromEmail}>";

        request.Content = JsonContent.Create(new
        {
            from,
            to = new[] { message.To },
            subject = message.Subject,
            html = message.Html,
            text = message.Text,
        });

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (Exception ex)
        {
            return Result.Fail("EMAIL_SEND_FAILED", $"Failed to call Resend API: {ex.Message}");
        }

        if (response.IsSuccessStatusCode)
            return Result.Ok();

        var responseBody = await response.Content.ReadAsStringAsync(ct);
        return Result.Fail(
            "EMAIL_SEND_FAILED",
            $"Resend returned status {(int)response.StatusCode}: {responseBody}");
    }

    private string ResolveApiKey()
    {
        if (!string.IsNullOrWhiteSpace(_settings.ApiKey))
            return _settings.ApiKey.Trim();

        return Environment.GetEnvironmentVariable("RESEND_API_KEY")?.Trim() ?? string.Empty;
    }
}
