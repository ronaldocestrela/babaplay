using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;
using BabaPlay.Infrastructure.Entities;
using BabaPlay.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Infrastructure.Repositories;

public sealed class AssociationInviteRepository : IAssociationInviteRepository
{
    private readonly MasterDbContext _context;

    public AssociationInviteRepository(MasterDbContext context) => _context = context;

    public async Task<AssociationInviteData?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
    {
        var entity = await _context.Set<AssociationInvite>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash, ct);

        return entity is null ? null : Map(entity);
    }

    public async Task<AssociationInviteData?> GetActiveByTenantAndEmailAsync(Guid tenantId, string normalizedEmail, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var entity = await _context.Set<AssociationInvite>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId &&
                x.NormalizedEmail == normalizedEmail &&
                x.AcceptedAtUtc == null &&
                x.RevokedAtUtc == null &&
                x.ExpiresAtUtc >= now,
                ct);

        return entity is null ? null : Map(entity);
    }

    public async Task AddAsync(AssociationInviteData invite, CancellationToken ct = default)
    {
        _context.Set<AssociationInvite>().Add(new AssociationInvite
        {
            Id = invite.Id,
            TenantId = invite.TenantId,
            Email = invite.Email,
            NormalizedEmail = invite.NormalizedEmail,
            TokenHash = invite.TokenHash,
            ExpiresAtUtc = invite.ExpiresAtUtc,
            CreatedAtUtc = invite.CreatedAtUtc,
            InvitedByUserId = invite.InvitedByUserId,
            AcceptedAtUtc = invite.AcceptedAtUtc,
            AcceptedByUserId = invite.AcceptedByUserId,
            RevokedAtUtc = invite.RevokedAtUtc,
        });

        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAcceptedAsync(Guid invitationId, string acceptedByUserId, DateTime acceptedAtUtc, CancellationToken ct = default)
    {
        var entity = await _context.Set<AssociationInvite>().FirstOrDefaultAsync(x => x.Id == invitationId, ct);
        if (entity is null)
            return;

        entity.AcceptedAtUtc = acceptedAtUtc;
        entity.AcceptedByUserId = acceptedByUserId;
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkRevokedAsync(Guid invitationId, DateTime revokedAtUtc, CancellationToken ct = default)
    {
        var entity = await _context.Set<AssociationInvite>().FirstOrDefaultAsync(x => x.Id == invitationId, ct);
        if (entity is null)
            return;

        entity.RevokedAtUtc = revokedAtUtc;
        await _context.SaveChangesAsync(ct);
    }

    private static AssociationInviteData Map(AssociationInvite x) => new(
        x.Id,
        x.TenantId,
        x.Email,
        x.NormalizedEmail,
        x.TokenHash,
        x.ExpiresAtUtc,
        x.CreatedAtUtc,
        x.InvitedByUserId,
        x.AcceptedAtUtc,
        x.AcceptedByUserId,
        x.RevokedAtUtc);
}
