namespace BabaPlay.Infrastructure.Settings;

public sealed class ResendEmailSettings
{
    public const string SectionName = "ResendEmail";

    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.resend.com";
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = string.Empty;
}
