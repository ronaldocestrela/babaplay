using BabaPlay.Application.Common;
using BabaPlay.Application.DTOs;
using BabaPlay.Application.Interfaces;

namespace BabaPlay.Application.Commands.Tenants;

public sealed class UpdateTenantSettingsCommandHandler
    : ICommandHandler<UpdateTenantSettingsCommand, Result<TenantResponse>>
{
    private static readonly HashSet<string> AllowedLogoContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg",
        "image/webp",
    };

    private const int MaxLogoBytes = 2 * 1024 * 1024;

    private readonly ITenantRepository _tenantRepository;
    private readonly IUserTenantRepository _userTenantRepository;
    private readonly ITenantLogoStorageService _tenantLogoStorageService;

    public UpdateTenantSettingsCommandHandler(
        ITenantRepository tenantRepository,
        IUserTenantRepository userTenantRepository,
        ITenantLogoStorageService tenantLogoStorageService)
    {
        _tenantRepository = tenantRepository;
        _userTenantRepository = userTenantRepository;
        _tenantLogoStorageService = tenantLogoStorageService;
    }

    public async Task<Result<TenantResponse>> HandleAsync(UpdateTenantSettingsCommand cmd, CancellationToken ct = default)
    {
        if (cmd.TenantId == Guid.Empty)
            return Result<TenantResponse>.Fail("TENANT_NOT_RESOLVED", "Tenant context is required.");

        if (string.IsNullOrWhiteSpace(cmd.RequestedByUserId))
            return Result<TenantResponse>.Fail("UNAUTHORIZED", "Authenticated user is required.");

        var isOwner = await _userTenantRepository.IsOwnerAsync(cmd.RequestedByUserId, cmd.TenantId, ct);
        if (!isOwner)
            return Result<TenantResponse>.Fail("FORBIDDEN", "Only tenant admins can edit tenant settings.");

        if (string.IsNullOrWhiteSpace(cmd.Name))
            return Result<TenantResponse>.Fail("TENANT_NAME_REQUIRED", "Tenant name is required.");

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

        string? logoPath = null;
        if (cmd.Logo is not null)
        {
            if (!AllowedLogoContentTypes.Contains(cmd.Logo.ContentType))
                return Result<TenantResponse>.Fail("TENANT_LOGO_INVALID_TYPE", "Logo must be PNG, JPEG, or WEBP.");

            if (cmd.Logo.Content.Length == 0 || cmd.Logo.Content.Length > MaxLogoBytes)
                return Result<TenantResponse>.Fail("TENANT_LOGO_INVALID_SIZE", "Logo must be between 1 byte and 2MB.");

            var logoStored = await _tenantLogoStorageService.SaveAsync(new TenantLogoSaveRequest(
                cmd.TenantId,
                cmd.Logo.FileName,
                cmd.Logo.ContentType,
                cmd.Logo.Content), ct);

            logoPath = logoStored.StoragePath;
        }

        var updated = await _tenantRepository.UpdateAssociationSettingsAsync(
            cmd.TenantId,
            cmd.Name.Trim(),
            logoPath,
            cmd.Street.Trim(),
            cmd.Number.Trim(),
            string.IsNullOrWhiteSpace(cmd.Neighborhood) ? null : cmd.Neighborhood.Trim(),
            cmd.City.Trim(),
            cmd.State.Trim(),
            cmd.ZipCode.Trim(),
            ct);

        if (!updated)
            return Result<TenantResponse>.Fail("TENANT_NOT_FOUND", $"Tenant '{cmd.TenantId}' was not found.");

        var tenant = await _tenantRepository.GetByIdAsync(cmd.TenantId, ct);
        if (tenant is null)
            return Result<TenantResponse>.Fail("TENANT_NOT_FOUND", $"Tenant '{cmd.TenantId}' was not found.");

        return Result<TenantResponse>.Ok(new TenantResponse(
            tenant.Id,
            tenant.Name,
            tenant.Slug,
            tenant.ProvisioningStatus,
            tenant.LogoPath,
            tenant.Street,
            tenant.Number,
            tenant.Neighborhood,
            tenant.City,
            tenant.State,
            tenant.ZipCode));
    }
}
