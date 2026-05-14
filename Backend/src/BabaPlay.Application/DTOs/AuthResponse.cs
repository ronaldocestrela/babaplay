namespace BabaPlay.Application.DTOs;

/// <summary>Tenant membership payload used by authentication/profile responses.</summary>
public sealed record AuthTenantMembershipDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsOwner,
    DateTime JoinedAt);

/// <summary>Authentication response returned after a successful login or token refresh.</summary>
public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string TokenType = "Bearer",
    AuthTenantMembershipDto? PrimaryTenant = null,
    IReadOnlyList<AuthTenantMembershipDto>? Tenants = null);

/// <summary>Current authenticated user profile for frontend bootstrap.</summary>
public sealed record UserProfileResponse(
    string Id,
    string Email,
    IReadOnlyCollection<string> Roles,
    bool IsActive,
    DateTime CreatedAt,
    AuthTenantMembershipDto? PrimaryTenant,
    IReadOnlyList<AuthTenantMembershipDto> Tenants);
