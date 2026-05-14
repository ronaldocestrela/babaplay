using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

public sealed class SendAssociationInviteCommandHandler
    : ICommandHandler<SendAssociationInviteCommand, Result<AssociationInviteResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly IAssociationInviteRepository _associationInviteRepository;
    private readonly IEmailDispatchQueue _emailDispatchQueue;

    public SendAssociationInviteCommandHandler(
        ITenantRepository tenantRepository,
        IUserTenantRepository userTenantRepository,
        IAssociationInviteRepository associationInviteRepository,
        IEmailDispatchQueue emailDispatchQueue)
    {
        _tenantRepository = tenantRepository;
        _userTenantRepository = userTenantRepository;
        _associationInviteRepository = associationInviteRepository;
        _emailDispatchQueue = emailDispatchQueue;
    }

    public async Task<Result<AssociationInviteResponse>> HandleAsync(SendAssociationInviteCommand cmd, CancellationToken ct = default)
    {
        if (cmd.TenantId == Guid.Empty)
            return Result<AssociationInviteResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        if (string.IsNullOrWhiteSpace(cmd.RequestedByUserId))
            return Result<AssociationInviteResponse>.Fail("UNAUTHORIZED", "Authenticated user is required.");

        if (string.IsNullOrWhiteSpace(cmd.Email))
            return Result<AssociationInviteResponse>.Fail("ASSOCIATION_INVITE_EMAIL_REQUIRED", "Invite e-mail is required.");

        var normalizedEmail = cmd.Email.Trim().ToLowerInvariant();
        if (!normalizedEmail.Contains('@'))
            return Result<AssociationInviteResponse>.Fail("ASSOCIATION_INVITE_EMAIL_INVALID", "Invite e-mail is invalid.");

        var tenant = await _tenantRepository.GetByIdAsync(cmd.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return Result<AssociationInviteResponse>.Fail("TENANT_NOT_FOUND", "Tenant not found.");

        var isOwner = await _userTenantRepository.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, ct);
        if (!isOwner)
            return Result<AssociationInviteResponse>.Fail("FORBIDDEN", "Only tenant owners can send invitations.");

        var existingActiveInvite = await _associationInviteRepository.GetActiveByTenantAndEmailAsync(cmd.TenantId, normalizedEmail, ct);
        if (existingActiveInvite is not null)
            await _associationInviteRepository.MarkRevokedAsync(existingActiveInvite.Id, DateTime.UtcNow, ct);

        var rawToken = AssociationInviteToken.GenerateRawToken();
        var tokenHash = AssociationInviteToken.ComputeHash(rawToken);

        var ttlHours = cmd.TokenExpiresInHours <= 0 ? 24 : cmd.TokenExpiresInHours;
        var expiresAtUtc = DateTime.UtcNow.AddHours(ttlHours);
        var invitationId = Guid.NewGuid();

        var invite = new AssociationInviteData(
            invitationId,
            cmd.TenantId,
            normalizedEmail,
            normalizedEmail,
            tokenHash,
            expiresAtUtc,
            DateTime.UtcNow,
            cmd.RequestedByUserId,
            null,
            null,
            null);

        await _associationInviteRepository.AddAsync(invite, ct);

        var acceptLink = BuildAcceptLink(cmd.AcceptLinkBaseUrl, rawToken);
        var html = $"<p>Voce foi convidado para entrar na associacao <strong>{tenant.Name}</strong>.</p><p><a href=\"{acceptLink}\">Clique aqui para aceitar o convite</a>.</p><p>Este link expira em 24 horas.</p>";

        await _emailDispatchQueue.EnqueueAsync(new EmailMessage(
            normalizedEmail,
            $"Convite para associacao {tenant.Name}",
            html,
            $"Acesse o link para aceitar o convite: {acceptLink}"),
            ct);

        return Result<AssociationInviteResponse>.Ok(new AssociationInviteResponse(
            invitationId,
            tenant.Id,
            tenant.Slug,
            normalizedEmail,
            expiresAtUtc));
    }

    private static string BuildAcceptLink(string baseUrl, string rawToken)
    {
        var normalizedBaseUrl = string.IsNullOrWhiteSpace(baseUrl)
            ? "http://localhost:5173/invite/accept"
            : baseUrl.Trim();

        var separator = normalizedBaseUrl.Contains('?') ? "&" : "?";
        return $"{normalizedBaseUrl}{separator}token={Uri.EscapeDataString(rawToken)}";
    }
}
