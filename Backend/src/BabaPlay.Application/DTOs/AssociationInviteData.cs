namespace BabaPlay.Application.DTOs;

public sealed record AssociationInviteData(
    Guid Id,
    Guid TenantId,
    string Email,
    string NormalizedEmail,
    string TokenHash,
    DateTime ExpiresAtUtc,
    DateTime CreatedAtUtc,
    string InvitedByUserId,
    DateTime? AcceptedAtUtc,
    string? AcceptedByUserId,
    DateTime? RevokedAtUtc);
