using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BabaPlay.Infrastructure.Messaging;

public sealed class ResendEmailService : IEmailService
{
    private const string ResendEndpoint = "emails";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IOptions<EmailSettings> _settings;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        HttpClient httpClient,
        IOptions<EmailSettings> settings,
        ILogger<ResendEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings;
        _logger = logger;
    }

    public async Task<Result<string>> SendAsync(
        string to,
        string subject,
        string htmlTemplate,
        IReadOnlyDictionary<string, string>? placeholders = null,
        string? fromEmail = null,
        string? fromName = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            return Result.Invalid<string>("Destination email is required.");

        if (string.IsNullOrWhiteSpace(subject))
            return Result.Invalid<string>("Email subject is required.");

        if (string.IsNullOrWhiteSpace(htmlTemplate))
            return Result.Invalid<string>("Email template is required.");

        var settings = _settings.Value;
        if (!settings.Enabled)
        {
            _logger.LogInformation("Email sending is disabled. Skipping send to {Email}.", to);
            return Result.Success("email-disabled");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
            return Result.Fail<string>("Email provider API key is not configured.");

        var senderEmail = string.IsNullOrWhiteSpace(fromEmail) ? settings.DefaultFromEmail : fromEmail;
        if (string.IsNullOrWhiteSpace(senderEmail))
            return Result.Fail<string>("Default sender email is not configured.");

        var senderName = string.IsNullOrWhiteSpace(fromName) ? settings.DefaultFromName : fromName;
        var renderedHtml = RenderTemplate(htmlTemplate, placeholders);

        var payload = new ResendRequest(
            BuildFrom(senderName, senderEmail),
            [to],
            subject,
            renderedHtml);

        using var request = new HttpRequestMessage(HttpMethod.Post, ResendEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Resend request failed with status {StatusCode} for {Email}. Body: {Body}",
                    (int)response.StatusCode,
                    to,
                    TrimForLog(content));

                return Result.Fail<string>("Failed to send email.");
            }

            var resendResponse = JsonSerializer.Deserialize<ResendResponse>(content, JsonOptions);
            var messageId = string.IsNullOrWhiteSpace(resendResponse?.Id) ? "sent" : resendResponse.Id;

            _logger.LogInformation("Email sent via Resend. MessageId: {MessageId}, To: {Email}", messageId, to);
            return Result.Success(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email with Resend to {Email}.", to);
            return Result.Fail<string>("Failed to send email.");
        }
    }

    private static string BuildFrom(string? senderName, string senderEmail)
    {
        if (string.IsNullOrWhiteSpace(senderName))
            return senderEmail;

        return $"{senderName} <{senderEmail}>";
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string>? placeholders)
    {
        if (placeholders is null || placeholders.Count == 0)
            return template;

        var output = template;
        foreach (var (key, value) in placeholders)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            output = output.Replace($"{{{{{key}}}}}", value ?? string.Empty, StringComparison.Ordinal);
        }

        return output;
    }

    private static string TrimForLog(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= 512 ? value : value[..512];
    }

    private sealed record ResendRequest(string From, string[] To, string Subject, string Html);

    private sealed record ResendResponse(string? Id);
}