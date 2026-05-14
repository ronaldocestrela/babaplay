using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Queries.Tenants;

public sealed class ValidateAssociationInviteQueryHandler
    : IQueryHandler<ValidateAssociationInviteQuery, Result<AssociationInviteValidationResponse>>
{
    private readonly IAssociationInviteRepository _associationInviteRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;

    public ValidateAssociationInviteQueryHandler(
        IAssociationInviteRepository associationInviteRepository,
        ITenantRepository tenantRepository,
        IUserRepository userRepository)
    {
        _associationInviteRepository = associationInviteRepository;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<AssociationInviteValidationResponse>> HandleAsync(ValidateAssociationInviteQuery query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query.Token))
            return Result<AssociationInviteValidationResponse>.Fail("ASSOCIATION_INVITE_INVALID_TOKEN", "Invite token is invalid.");

        var tokenHash = BabaPlay.Application.Commands.Tenants.AssociationInviteToken.ComputeHash(query.Token.Trim());
        var invite = await _associationInviteRepository.GetByTokenHashAsync(tokenHash, ct);

        var validationError = ValidateInviteState(invite);
        if (validationError is not null)
            return Result<AssociationInviteValidationResponse>.Fail(validationError.Value.ErrorCode, validationError.Value.ErrorMessage);

        var tenant = await _tenantRepository.GetByIdAsync(invite!.TenantId, ct);
        if (tenant is null || !tenant.IsActive)
            return Result<AssociationInviteValidationResponse>.Fail("TENANT_NOT_FOUND", "Tenant not found.");

        var existingUser = await _userRepository.FindByEmailAsync(invite.Email, ct);

        return Result<AssociationInviteValidationResponse>.Ok(new AssociationInviteValidationResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            invite.Email,
            invite.ExpiresAtUtc,
            existingUser is null));
    }

    private static (string ErrorCode, string ErrorMessage)? ValidateInviteState(AssociationInviteData? invite)
    {
        if (invite is null)
            return ("ASSOCIATION_INVITE_INVALID_TOKEN", "Invite token is invalid.");

        if (invite.RevokedAtUtc.HasValue)
            return ("ASSOCIATION_INVITE_ALREADY_REVOKED", "Invite has already been revoked.");

        if (invite.AcceptedAtUtc.HasValue)
            return ("ASSOCIATION_INVITE_ALREADY_USED", "Invite has already been used.");

        if (invite.ExpiresAtUtc < DateTime.UtcNow)
            return ("ASSOCIATION_INVITE_TOKEN_EXPIRED", "Invite token has expired.");

        return null;
    }
}
