using System.Security.Cryptography;
using BabaPlay.Modules.Associates.Entities;
using BabaPlay.SharedKernel.Repositories;
using BabaPlay.SharedKernel.Results;
using BabaPlay.SharedKernel.Security;
using Microsoft.EntityFrameworkCore;

namespace BabaPlay.Modules.Associates.Services;

public sealed class AssociateInvitationService : IAssociateInvitationService
{
    private readonly ITenantRepository<AssociateInvitation> _invitations;
    private readonly ITenantUnitOfWork _uow;

    public AssociateInvitationService(
        ITenantRepository<AssociateInvitation> invitations,
        ITenantUnitOfWork uow)
    {
        _invitations = invitations;
        _uow = uow;
    }

    public async Task<Result<AssociateInvitationIssueResult>> CreateAsync(
        string? email,
        bool isSingleUse,
        string invitedByUserId,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        if (isSingleUse && string.IsNullOrWhiteSpace(email))
            return Result.Invalid<AssociateInvitationIssueResult>("Email is required for single-use invitations.");

        if (string.IsNullOrWhiteSpace(invitedByUserId))
            return Result.Unauthorized<AssociateInvitationIssueResult>("Authenticated user is required.");

        if (ttl <= TimeSpan.Zero)
            return Result.Invalid<AssociateInvitationIssueResult>("Invitation expiration must be greater than zero.");

        var now = DateTime.UtcNow;
        var normalizedEmail = email?.Trim();

        if (isSingleUse)
        {
            var hasPending = await _invitations.Query().AnyAsync(
                x => x.IsSingleUse && x.Email == normalizedEmail && x.AcceptedAt == null && x.ExpiresAt > now,
                cancellationToken);

            if (hasPending)
                return Result.Conflict<AssociateInvitationIssueResult>("A pending invitation already exists for this email.");
        }

        var invitation = new AssociateInvitation
        {
            Email = normalizedEmail,
            IsSingleUse = isSingleUse,
            Token = GenerateToken(),
            InvitedByUserId = invitedByUserId,
            ExpiresAt = now.Add(ttl)
        };

        await _invitations.AddAsync(invitation, cancellationToken);

        try
        {
            await _uow.SaveChangesAsync(cancellationToken);
            return Result.Success(new AssociateInvitationIssueResult(invitation.Token, invitation.Email, invitation.ExpiresAt));
        }
        catch (DbUpdateException)
        {
            return Result.Conflict<AssociateInvitationIssueResult>("Could not generate invitation token.");
        }
    }

    public async Task<Result<AssociateInvitationValidationResult>> ValidateAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result.Invalid<AssociateInvitationValidationResult>("Invitation token is required.");

        var invitation = await _invitations.Query()
            .FirstOrDefaultAsync(x => x.Token == token.Trim(), cancellationToken);

        if (invitation is null)
            return Result.NotFound<AssociateInvitationValidationResult>("Invitation not found.");

        if (invitation.IsSingleUse && invitation.AcceptedAt is not null)
            return Result.Conflict<AssociateInvitationValidationResult>("Invitation already used.");

        if (invitation.ExpiresAt <= DateTime.UtcNow)
            return Result.Invalid<AssociateInvitationValidationResult>("Invitation expired.");

        return Result.Success(new AssociateInvitationValidationResult(
            invitation.Token,
            invitation.Email,
            invitation.IsSingleUse,
            invitation.ExpiresAt));
    }

    public async Task<Result> MarkAcceptedAsync(
        string token,
        string acceptedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure("Invitation token is required.", ResultStatus.Invalid);

        if (string.IsNullOrWhiteSpace(acceptedByUserId))
            return Result.Failure("Accepted user id is required.", ResultStatus.Invalid);

        var invitation = await _invitations.Query()
            .FirstOrDefaultAsync(x => x.Token == token.Trim(), cancellationToken);

        if (invitation is null)
            return Result.Failure("Invitation not found.", ResultStatus.NotFound);

        if (invitation.IsSingleUse && invitation.AcceptedAt is not null)
            return Result.Failure("Invitation already used.", ResultStatus.Conflict);

        if (invitation.ExpiresAt <= DateTime.UtcNow)
            return Result.Failure("Invitation expired.", ResultStatus.Invalid);

        if (invitation.IsSingleUse)
        {
            invitation.AcceptedAt = DateTime.UtcNow;
            invitation.AcceptedByUserId = acceptedByUserId;
        }

        invitation.UsesCount += 1;
        invitation.UpdatedAt = DateTime.UtcNow;

        _invitations.Update(invitation);
        await _uow.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static string GenerateToken()
    {
        Span<byte> buffer = stackalloc byte[20];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToHexString(buffer).ToLowerInvariant();
    }
}
