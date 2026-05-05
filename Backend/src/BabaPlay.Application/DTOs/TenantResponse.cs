namespace BabaPlay.Application.DTOs;

/// <summary>API response returned after a successful tenant creation request.</summary>
public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string ProvisioningStatus,
    string? LogoPath = null,
    string? Street = null,
    string? Number = null,
    string? Neighborhood = null,
    string? City = null,
    string? State = null,
    string? ZipCode = null);
