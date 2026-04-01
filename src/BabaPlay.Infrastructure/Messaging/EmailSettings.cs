namespace BabaPlay.Infrastructure.Messaging;

public sealed class EmailSettings
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; } = false;
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultFromEmail { get; set; } = string.Empty;
    public string DefaultFromName { get; set; } = "BabaPlay";
}