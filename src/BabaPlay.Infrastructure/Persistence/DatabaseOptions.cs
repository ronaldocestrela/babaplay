namespace BabaPlay.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Connection string for the central platform catalog (metadata DB).</summary>
    public string PlatformConnectionString { get; set; } = string.Empty;

    /// <summary>Optional template used only by tools/migrations for tenant model.</summary>
    public string TenantTemplateConnectionString { get; set; } = string.Empty;
}
