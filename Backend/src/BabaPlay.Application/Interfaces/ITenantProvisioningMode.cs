namespace BabaPlay.Application.Interfaces;

/// <summary>
/// Exposes the current tenancy provisioning mode.
/// </summary>
public interface ITenantProvisioningMode
{
    /// <summary>
    /// True when legacy per-tenant database provisioning is enabled.
    /// False when single-database tenancy is enabled.
    /// </summary>
    bool UseTenantDatabaseProvisioning { get; }
}
