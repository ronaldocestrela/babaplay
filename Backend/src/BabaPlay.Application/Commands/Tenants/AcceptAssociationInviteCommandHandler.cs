using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

public sealed class AcceptAssociationInviteCommandHandler
    : ICommandHandler<AcceptAssociationInviteCommand, Result<AssociationInviteAcceptResponse>>,
      ICommandHandler<RegisterAndAcceptAssociationInviteCommand, Result<AssociationInviteAcceptResponse>>
{
    private readonly IAssociationInviteRepository _associationInviteRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserInvitationAccountService _userInvitationAccountService;
    private readonly IUserTenantMembershipService _userTenantMembershipService;
    private readonly IPlayerOnboardingReadService _playerOnboardingReadService;

    public AcceptAssociationInviteCommandHandler(
        IAssociationInviteRepository associationInviteRepository,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IUserInvitationAccountService userInvitationAccountService,
        IUserTenantMembershipService userTenantMembershipService,
        IPlayerOnboardingReadService playerOnboardingReadService)
    {
        _associationInviteRepository = associationInviteRepository;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _userInvitationAccountService = userInvitationAccountService;
        _userTenantMembershipService = userTenantMembershipService;
        _playerOnboardingReadService = playerOnboardingReadService;
    }

    public async Task<Result<AssociationInviteAcceptResponse>> HandleAsync(AcceptAssociationInviteCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserId))
            return Result<AssociationInviteAcceptResponse>.Fail("UNAUTHORIZED", "Authenticated user is required.");

        var inviteResult = await ResolveValidInviteAsync(command.Token, ct);
        if (!inviteResult.IsSuccess)
            return Result<AssociationInviteAcceptResponse>.Fail(inviteResult.ErrorCode!, inviteResult.ErrorMessage!);

        var invite = inviteResult.Value!;
        var user = await _userRepository.FindByIdAsync(command.UserId, ct);
        if (user is null)
            return Result<AssociationInviteAcceptResponse>.Fail("UNAUTHORIZED", "Authenticated user was not found.");

        if (!string.Equals(user.Email, invite.Email, StringComparison.OrdinalIgnoreCase))
            return Result<AssociationInviteAcceptResponse>.Fail("ASSOCIATION_INVITE_EMAIL_MISMATCH", "Invite e-mail does not match authenticated user.");

        return await AcceptForUserAsync(invite, user.Id, user.Email, ct);
    }

    public async Task<Result<AssociationInviteAcceptResponse>> HandleAsync(RegisterAndAcceptAssociationInviteCommand command, CancellationToken ct = default)
    {
        var inviteResult = await ResolveValidInviteAsync(command.Token, ct);
        if (!inviteResult.IsSuccess)
            return Result<AssociationInviteAcceptResponse>.Fail(inviteResult.ErrorCode!, inviteResult.ErrorMessage!);

        var invite = inviteResult.Value!;

        if (string.IsNullOrWhiteSpace(command.Email) || !string.Equals(command.Email.Trim(), invite.Email, StringComparison.OrdinalIgnoreCase))
            return Result<AssociationInviteAcceptResponse>.Fail("ASSOCIATION_INVITE_EMAIL_MISMATCH", "Invite e-mail does not match requested account e-mail.");

        if (string.IsNullOrWhiteSpace(command.Password))
            return Result<AssociationInviteAcceptResponse>.Fail("ASSOCIATION_INVITE_PASSWORD_REQUIRED", "Password is required to register by invitation.");

        var existingUser = await _userRepository.FindByEmailAsync(invite.Email, ct);
        if (existingUser is not null)
            return Result<AssociationInviteAcceptResponse>.Fail("ASSOCIATION_INVITE_EMAIL_ALREADY_REGISTERED", "This e-mail is already registered. Please login to accept the invite.");

        var createResult = await _userInvitationAccountService.CreateUserAsync(invite.Email, command.Password, ct);
        if (!createResult.IsSuccess)
            return Result<AssociationInviteAcceptResponse>.Fail(createResult.ErrorCode!, createResult.ErrorMessage!);

        return await AcceptForUserAsync(invite, createResult.Value!, invite.Email, ct);
    }

    private async Task<Result<AssociationInviteData>> ResolveValidInviteAsync(string rawToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            return Result<AssociationInviteData>.Fail("ASSOCIATION_INVITE_INVALID_TOKEN", "Invite token is invalid.");

        var tokenHash = AssociationInviteToken.ComputeHash(rawToken.Trim());
        var invite = await _associationInviteRepository.GetByTokenHashAsync(tokenHash, ct);

        if (invite is null)
            return Result<AssociationInviteData>.Fail("ASSOCIATION_INVITE_INVALID_TOKEN", "Invite token is invalid.");

        if (invite.RevokedAtUtc.HasValue)
            return Result<AssociationInviteData>.Fail("ASSOCIATION_INVITE_ALREADY_REVOKED", "Invite has already been revoked.");

        if (invite.AcceptedAtUtc.HasValue)
            return Result<AssociationInviteData>.Fail("ASSOCIATION_INVITE_ALREADY_USED", "Invite has already been used.");

        if (invite.ExpiresAtUtc < DateTime.UtcNow)
            return Result<AssociationInviteData>.Fail("ASSOCIATION_INVITE_TOKEN_EXPIRED", "Invite token has expired.");

        return Result<AssociationInviteData>.Ok(invite);
    }

    private async Task<Result<AssociationInviteAcceptResponse>> AcceptForUserAsync(
        AssociationInviteData invite,
        string userId,
        string userEmail,
        CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(invite.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return Result<AssociationInviteAcceptResponse>.Fail("TENANT_NOT_FOUND", "Tenant not found.");

        var alreadyMember = await _userTenantMembershipService.EnsureMemberAsync(userId, invite.TenantId, ct);
        var hasActivePlayerProfile = await _playerOnboardingReadService.HasActivePlayerProfileAsync(invite.TenantId, userId, ct);
        var requiresPlayerProfile = !hasActivePlayerProfile;

        await _associationInviteRepository.MarkAcceptedAsync(invite.Id, userId, DateTime.UtcNow, ct);

        return Result<AssociationInviteAcceptResponse>.Ok(new AssociationInviteAcceptResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            userId,
            userEmail,
            requiresPlayerProfile,
            alreadyMember));
    }
}
