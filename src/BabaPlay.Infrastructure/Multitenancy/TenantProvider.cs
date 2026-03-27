namespace BabaPlay.Infrastructure.Multitenancy;

public sealed class TenantProvider : ITenantProvider
{
    private static readonly AsyncLocal<TenantHolder?> Holder = new();

    public string? TenantId => Holder.Value?.TenantId;
    public string? TenantConnectionString => Holder.Value?.ConnectionString;
    public bool IsPlatformRequest => Holder.Value?.IsPlatform ?? false;

    public void SetPlatformContext()
    {
        Holder.Value = new TenantHolder { IsPlatform = true };
    }

    public void SetTenant(string tenantId, string connectionString)
    {
        Holder.Value = new TenantHolder
        {
            IsPlatform = false,
            TenantId = tenantId,
            ConnectionString = connectionString
        };
    }

    public void Clear()
    {
        Holder.Value = null;
    }

    private sealed class TenantHolder
    {
        public bool IsPlatform { get; init; }
        public string? TenantId { get; init; }
        public string? ConnectionString { get; init; }
    }
}
