using BabaPlay.Application.DTOs;

namespace BabaPlay.Application.Interfaces;

public interface ITenantGeolocationSettingsRepository
{
    Task<TenantGeolocationSettingsDto?> GetSettingsAsync(Guid tenantId, CancellationToken ct = default);
}
