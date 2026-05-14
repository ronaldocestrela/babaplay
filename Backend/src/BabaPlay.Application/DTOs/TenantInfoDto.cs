namespace BabaPlay.Application.DTOs;

/// <summary>Lightweight tenant descriptor returned by repository lookups.</summary>
public sealed record TenantInfoDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsActive,
    string ConnectionString,
    string ProvisioningStatus,
    int PlayersPerTeam = 11,
    string? LogoPath = null,
    string? Street = null,
    string? Number = null,
    string? Neighborhood = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null,
    double? AssociationLatitude = null,
    double? AssociationLongitude = null);
