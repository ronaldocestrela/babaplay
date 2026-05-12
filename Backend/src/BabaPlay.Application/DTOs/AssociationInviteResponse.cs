namespace BabaPlay.Application.DTOs;

public sealed record AssociationInviteResponse(
    Guid InvitationId,
    Guid TenantId,
    string TenantSlug,
    string Email,
    DateTime ExpiresAtUtc);

public sealed record AssociationInviteValidationResponse(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    string Email,
    DateTime ExpiresAtUtc,
    bool RequiresRegistration);

public sealed record AssociationInviteAcceptResponse(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    string UserId,
    string Email,
    bool AlreadyMember);
