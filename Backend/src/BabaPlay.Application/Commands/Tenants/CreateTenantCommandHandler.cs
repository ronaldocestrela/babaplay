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

    public CreateTenantCommandHandler(
        ITenantRepository tenantRepository,
        ITenantProvisioningQueue provisioningQueue)
    {
        _tenantRepository = tenantRepository;
        _provisioningQueue = provisioningQueue;
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

        var slugNormalised = cmd.Slug.Trim().ToLowerInvariant();

        if (await _tenantRepository.ExistsAsync(slugNormalised, ct))
            return Result<TenantResponse>.Fail("TENANT_SLUG_TAKEN", $"Slug '{slugNormalised}' is already taken.");

        var tenantId = Guid.NewGuid();
        await _tenantRepository.AddAsync(tenantId, cmd.Name.Trim(), slugNormalised, ct);
        await _provisioningQueue.EnqueueAsync(tenantId, ct);

        return Result<TenantResponse>.Ok(new TenantResponse(
            tenantId,
            cmd.Name.Trim(),
            slugNormalised,
            "Pending"));
    }
}
