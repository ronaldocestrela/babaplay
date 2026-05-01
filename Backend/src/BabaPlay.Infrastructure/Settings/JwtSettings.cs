namespace BabaPlay.Infrastructure.Settings;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenExpiresInMinutes { get; init; } = 60;
    public int RefreshTokenExpiresInDays { get; init; } = 30;
}
