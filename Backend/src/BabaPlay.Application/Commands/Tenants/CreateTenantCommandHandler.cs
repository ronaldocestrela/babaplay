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
    private static readonly HashSet<string> AllowedLogoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp",
    };

    private const int MaxLogoBytes = 2 * 1024 * 1024;

    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantProvisioningQueue _provisioningQueue;
    private readonly ITenantOwnerProvisioningService _tenantOwnerProvisioningService;
    private readonly ITenantLogoStorageService _tenantLogoStorageService;

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ITenantProvisioningQueue provisioningQueue,
        ITenantOwnerProvisioningService tenantOwnerProvisioningService,
        ITenantLogoStorageService tenantLogoStorageService)
    {
        _tenantRepository = tenantRepository;
        _provisioningQueue = provisioningQueue;
        _tenantOwnerProvisioningService = tenantOwnerProvisioningService;
        _tenantLogoStorageService = tenantLogoStorageService;
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

        if (cmd.Logo is null)
            return Result<TenantResponse>.Fail("TENANT_LOGO_REQUIRED", "Association logo is required.");

        if (!AllowedLogoContentTypes.Contains(cmd.Logo.ContentType))
            return Result<TenantResponse>.Fail("TENANT_LOGO_INVALID_TYPE", "Logo must be PNG, JPEG, or WEBP.");

        if (cmd.Logo.Content.Length == 0 || cmd.Logo.Content.Length > MaxLogoBytes)
            return Result<TenantResponse>.Fail("TENANT_LOGO_INVALID_SIZE", "Logo must be between 1 byte and 2MB.");

        if (string.IsNullOrWhiteSpace(cmd.Street))
            return Result<TenantResponse>.Fail("TENANT_STREET_REQUIRED", "Street is required.");

        if (string.IsNullOrWhiteSpace(cmd.Number))
            return Result<TenantResponse>.Fail("TENANT_NUMBER_REQUIRED", "Number is required.");

        if (string.IsNullOrWhiteSpace(cmd.City))
            return Result<TenantResponse>.Fail("TENANT_CITY_REQUIRED", "City is required.");

        if (string.IsNullOrWhiteSpace(cmd.State))
            return Result<TenantResponse>.Fail("TENANT_STATE_REQUIRED", "State is required.");

        if (string.IsNullOrWhiteSpace(cmd.ZipCode))
            return Result<TenantResponse>.Fail("TENANT_ZIPCODE_REQUIRED", "Zip code is required.");

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
        var logoStored = await _tenantLogoStorageService.SaveAsync(new TenantLogoSaveRequest(
            tenantId,
            cmd.Logo.FileName,
            cmd.Logo.ContentType,
            cmd.Logo.Content), ct);

        await _tenantRepository.AddAsync(
            tenantId,
            cmd.Name.Trim(),
            slugNormalised,
            logoStored.StoragePath,
            cmd.Street.Trim(),
            cmd.Number.Trim(),
            string.IsNullOrWhiteSpace(cmd.Neighborhood) ? null : cmd.Neighborhood.Trim(),
            cmd.City.Trim(),
            cmd.State.Trim(),
            cmd.ZipCode.Trim(),
            ct);

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
            "Pending",
            11,
            logoStored.StoragePath,
            cmd.Street.Trim(),
            cmd.Number.Trim(),
            string.IsNullOrWhiteSpace(cmd.Neighborhood) ? null : cmd.Neighborhood.Trim(),
            cmd.City.Trim(),
            cmd.State.Trim(),
            cmd.ZipCode.Trim()));
    }
}
