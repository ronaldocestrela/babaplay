using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

/// <summary>
/// Handles tenant creation: validates slug uniqueness, persists tenant record
/// with Pending status, and enqueues the database provisioning job.
/// </summary>
public sealed class CreateTenantCommandHandler
    : ICommandHandler<CreateTenantCommand, Result<TenantResponse>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantProvisioningQueue _provisioningQueue;
    private readonly ITenantOwnerProvisioningService _tenantOwnerProvisioningService;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ITenantProvisioningQueue provisioningQueue,
        ITenantOwnerProvisioningService tenantOwnerProvisioningService)
    {
        _tenantRepository = tenantRepository;
        _provisioningQueue = provisioningQueue;
        _tenantOwnerProvisioningService = tenantOwnerProvisioningService;
    }

    /// <inheritdoc />
    public async Task<Result<TenantResponse>> HandleAsync(
        CreateTenantCommand cmd,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<TenantResponse>.Fail("TENANT_NAME_REQUIRED", "Tenant name is required.");

        if (string.IsNullOrWhiteSpace(cmd.Slug))
            return Result<TenantResponse>.Fail("TENANT_SLUG_REQUIRED", "Tenant slug is required.");

        var isAnonymousFlow = string.IsNullOrWhiteSpace(cmd.RequestedByUserId);
        if (isAnonymousFlow &&
            (string.IsNullOrWhiteSpace(cmd.AdminEmail) || string.IsNullOrWhiteSpace(cmd.AdminPassword)))
        {
            return Result<TenantResponse>.Fail(
                "TENANT_ADMIN_CREDENTIALS_REQUIRED",
                "Admin email and password are required to bootstrap the tenant owner.");
        }

        var slugNormalised = cmd.Slug.Trim().ToLowerInvariant();

        if (await _tenantRepository.ExistsAsync(slugNormalised, ct))
            return Result<TenantResponse>.Fail("TENANT_SLUG_TAKEN", $"Slug '{slugNormalised}' is already taken.");

        var ownerResult = await _tenantOwnerProvisioningService.ResolveOwnerUserIdAsync(
            cmd.RequestedByUserId,
            cmd.AdminEmail,
            cmd.AdminPassword,
            ct);

        if (!ownerResult.IsSuccess)
            return Result<TenantResponse>.Fail(ownerResult.ErrorCode!, ownerResult.ErrorMessage!);

        var tenantId = Guid.NewGuid();
        await _tenantRepository.AddAsync(tenantId, cmd.Name.Trim(), slugNormalised, ct);

        var membershipResult = await _tenantOwnerProvisioningService.EnsureOwnerMembershipAsync(
            ownerResult.Value!,
            tenantId,
            ct);

        if (!membershipResult.IsSuccess)
            return Result<TenantResponse>.Fail(membershipResult.ErrorCode!, membershipResult.ErrorMessage!);

        await _provisioningQueue.EnqueueAsync(tenantId, ct);

        return Result<TenantResponse>.Ok(new TenantResponse(
            tenantId,
            cmd.Name.Trim(),
            slugNormalised,
            "Pending"));
    }
}
