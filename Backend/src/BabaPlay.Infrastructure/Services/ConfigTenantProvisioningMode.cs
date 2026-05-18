using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace BabaPlay.Infrastructure.Services;

public sealed class ConfigTenantProvisioningMode : ITenantProvisioningMode
{
    private readonly TenancySettings _settings;

    public ConfigTenantProvisioningMode(IOptions<TenancySettings> settings)
        => _settings = settings.Value;

    public bool UseTenantDatabaseProvisioning => _settings.UseTenantDatabaseProvisioning;
}
