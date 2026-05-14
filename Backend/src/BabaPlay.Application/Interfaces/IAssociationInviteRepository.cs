using BabaPlay.Application.DTOs;

namespace BabaPlay.Application.Interfaces;

public interface IAssociationInviteRepository
{
    Task<AssociationInviteData?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default);

    Task<AssociationInviteData?> GetActiveByTenantAndEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken ct = default);

    Task AddAsync(AssociationInviteData invite, CancellationToken ct = default);

    Task MarkAcceptedAsync(Guid invitationId, string acceptedByUserId, DateTime acceptedAtUtc, CancellationToken ct = default);

    Task MarkRevokedAsync(Guid invitationId, DateTime revokedAtUtc, CancellationToken ct = default);
}
