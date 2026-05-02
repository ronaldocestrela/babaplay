namespace BabaPlay.Application.DTOs;

/// <summary>Lightweight tenant descriptor returned by repository lookups.</summary>
public sealed record TenantInfoDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    string ConnectionString,
    string ProvisioningStatus);
