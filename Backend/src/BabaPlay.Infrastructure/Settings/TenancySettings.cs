namespace BabaPlay.Infrastructure.Settings;

public sealed class TenancySettings
{
    public const string SectionName = "Tenancy";

    /// <summary>
    /// Enables legacy provisioning where each tenant has an isolated database.
    /// Set to false to use single-database tenancy.
    /// </summary>
    public bool UseTenantDatabaseProvisioning { get; set; } = true;
}
