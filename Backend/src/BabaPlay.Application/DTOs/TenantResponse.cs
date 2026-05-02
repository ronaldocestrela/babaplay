namespace BabaPlay.Application.DTOs;

/// <summary>API response returned after a successful tenant creation request.</summary>
public sealed record TenantResponse(
    Guid Id,
    string Name,
    string Slug,
    string ProvisioningStatus);
